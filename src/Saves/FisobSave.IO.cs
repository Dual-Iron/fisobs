using Mono.Cecil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace Fisobs.Saves
{
    partial struct FisobSave
    {
        // NOTE: Not thread-safe when writing save files.

        private const string filename = "fisobs.dat";
        private const ushort currentVersion = 0;
        private static byte[] Magic => new byte[] { 0x46, 0x49, 0x53, 0x4F, 0x42, 0x53 };

        #region READ
        public static FisobSave ReadOrCreate()
        {
            var read = Read();
            if (read.success is FisobSave save) {
                return save;
            } else {
                Debug.LogError($"Couldn't read {filename}: {read.err}");

                return new(new());
            }
        }

        public static ReadResult Read()
        {
            string dir = Path.Combine(Custom.RootFolderDirectory(), "UserData");
            string path = Path.Combine(dir, filename);

            try {
                Directory.CreateDirectory(dir);

                if (File.Exists(path)) {
                    using Stream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return FromStream(fs);
                } else {
                    return new FisobSave(new());
                }
            } catch (Exception e) {
                Debug.LogError($"Exception thrown while writing {filename}. {e.Message}");
                Debug.LogException(e);

                return ReadError.IOError;
            }
        }

        static ReadResult FromStream(Stream stream)
        {
            if (stream.Length < 40) {
                return ReadError.TooShort;
            }

            byte[] magicNumber = new byte[6];
            stream.Read(magicNumber, 0, 6);
            if (!magicNumber.SequenceEqual(Magic)) {
                return ReadError.InvalidFormat;
            }

            int version = stream.ReadU16()!.Value;
            return version switch {
                0 => ReadV0(stream),
                _ => ReadError.FutureVersion,
            };
        }

        static ReadResult ReadV0(Stream stream)
        {
            // Read checksum
            byte[] checksum = new byte[32];
            if (stream.Read(checksum, 0, 32) != 32) {
                return ReadError.TooShort;
            }

            long position = stream.Position;

            using (var hash = SHA256.Create()) {
                if (!hash.ComputeHash(stream).SequenceEqual(checksum)) {
                    return ReadError.TamperedWith;
                }
            }

            stream.Position = position;

            Dictionary<string, FisobSaveSlot> slots = new();

            while (stream.Position < stream.Length) {

                // Read one save slot
                if (stream.ReadStr() is string slotName && stream.ReadU16() is int entries) {

                    // Read entries in save slot
                    List<string> unlocked = new();
                    for (int i = 0; i < entries; i++) {
                        if (stream.ReadStr() is string entry) {
                            unlocked.Add(entry);
                        } else {
                            return ReadError.TooShort;
                        }
                    }

                    slots[slotName] = new(unlocked);

                } else {
                    return ReadError.TooShort;
                }
            }

            return stream.Position == stream.Length ? new FisobSave(slots) : ReadError.TooShort;
        }
        #endregion

        #region WRITE
        public void WriteOrLogError()
        {
            var err = Write();
            if (err != WriteError.None) {
                Debug.LogError($"Couldn't write {filename}: {err}");
            }
        }

        public WriteError Write()
        {
            try {
                string dir = Path.Combine(Custom.RootFolderDirectory(), "UserData");
                string path = Path.Combine(dir, filename);

                Directory.CreateDirectory(dir);

                using Stream fs = File.Open(path, FileMode.Create, FileAccess.Write);

                return WriteCurrentVersion(fs);
            } catch (Exception e) {
                Debug.LogError($"Exception thrown while writing {filename}. {e.Message}");
                Debug.LogException(e);

                return WriteError.IOError;
            }
        }

        private WriteError WriteCurrentVersion(Stream stream)
        {
            using MemoryStream ms = new();

            // Write save slots early so hash can be computed
            foreach (var slot in slots) {
                ms.WriteStr(slot.Key);
                ms.WriteU16(slot.Value.Unlocked.Count);
                foreach (var unlock in slot.Value.Unlocked) {
                    ms.WriteStr(unlock);
                }
            }
            ms.Position = 0;

            using var hash = SHA256.Create();
            byte[] hashBytes = hash.ComputeHash(ms);
            ms.Position = 0;

            // offset   value
            stream.Write(Magic, 0, 6);   // 0        magic number
            stream.WriteU16(currentVersion);        // 6        version
            stream.Write(hashBytes, 0, 32);         // 8        sha-256 checksum
            ms.CopyTo(stream);                      // 40       save slots

            return WriteError.None;
        }
        #endregion
    }
}
