using static PlayerProgression;
using UnityEngine;
using System;
using ArenaBehaviors;
using System.Linq;
using Fisobs.Core;
using Fisobs.Saves;
using System.Collections.Generic;

namespace Fisobs.Sandbox
{
    public sealed partial class SandboxRegistry : Registry
    {
        private void SpawnEntity(On.SandboxGameSession.orig_SpawnEntity orig, SandboxGameSession self, SandboxEditor.PlacedIconData p)
        {
            EntityID id = self.GameTypeSetup.saveCreatures ? p.ID : self.game.GetNewID();
            WorldCoordinate coord = new(0, Mathf.RoundToInt(p.pos.x / 20f), Mathf.RoundToInt(p.pos.y / 20f), -1);

            if (sboxes.TryGetValue(p.data.critType, out var critob)) {
                EntitySaveData? data = null;

                if (self.GameTypeSetup.saveCreatures) {
                    var creature = self.arenaSitting.creatures.FirstOrDefault(c => c.creatureTemplate.type == p.data.critType && c.ID == p.ID);
                    if (creature != null) {
                        self.arenaSitting.creatures.Remove(creature);

                        for (int i = 0; i < 2; i++) {
                            creature.state.CycleTick();
                        }

                        data = EntitySaveData.CreateFrom(creature, creature.state.ToString());
                    }
                }

                DoSpawn(self, p, data ?? new(p.data.critType, id, coord, ""), critob);

            } else if (sboxes.TryGetValue(p.data.itemType, out var fisob)) {
                EntitySaveData data = new(p.data.itemType, id, coord, "");

                DoSpawn(self, p, data, fisob);

            } else {
                orig(self, p);
            }
        }

        private static void DoSpawn(SandboxGameSession self, SandboxEditor.PlacedIconData p, EntitySaveData data, ISandboxHandler handler)
        {
            SandboxUnlock unlock = handler.SandboxUnlocks.FirstOrDefault(u => u.Data == p.data.intData);

            if (!unlock.IsInitialized) {
                Debug.LogError($"The fisob \"{handler.Type}\" had no sandbox unlocks where Data={p.data.intData}.");
                return;
            }

            try {
                var entity = handler.ParseFromSandbox(self.game.world, data, unlock);
                if (entity != null) {
                    self.game.world.GetAbstractRoom(0).AddEntity(entity);
                } else {
                    Debug.LogError($"The sandbox unlock \"{unlock.Type}\" returned null when being parsed in sandbox mode.");
                }
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"The sandbox unlock \"{unlock.Type}\" threw an exception when being parsed in sandbox mode.");
            }
        }

        private IconSymbol.IconSymbolData FromUnlock(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            try {
                return orig(unlockID);
            } catch {
                foreach (var common in sboxes.Values) {
                    var unlock = common.SandboxUnlocks.FirstOrDefault(u => u.Type == unlockID);
                    if (unlock.IsInitialized) {
                        return new(common.Type.CritType, common.Type.ObjectType, unlock.Data);
                    }
                }
                throw;
            }
        }

        private MultiplayerUnlocks.SandboxUnlockID FromSymbolData(On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data)
        {
            if (data.itemType < 0 || data.itemType > AbstractPhysicalObject.AbstractObjectType.OverseerCarcass) {
                if (sboxes.TryGetValue(data.itemType, out var item) && item.SandboxUnlocks.FirstOrDefault(u => u.Data == data.intData) is SandboxUnlock unlock) {
                    return unlock.Type;
                }
            } else if (data.critType < 0 || data.critType > CreatureTemplate.Type.Hazer) {
                if (sboxes.TryGetValue(data.critType, out var crit) && crit.SandboxUnlocks.FirstOrDefault(u => u.Data == data.intData) is SandboxUnlock unlock) {
                    return unlock.Type;
                }
            }
            return orig(data);
        }

        private MultiplayerUnlocks.SandboxUnlockID? GetParent(On.MultiplayerUnlocks.orig_ParentSandboxID orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            foreach (var common in sboxes.Values) {
                var unlock = common.SandboxUnlocks.FirstOrDefault(s => s.Type == unlockID);
                if (unlock.IsInitialized) {
                    return unlock.Parent;
                }
            }
            return orig(unlockID);
        }

        private List<MultiplayerUnlocks.SandboxUnlockID> TiedSandboxIDs(On.MultiplayerUnlocks.orig_TiedSandboxIDs orig, MultiplayerUnlocks.SandboxUnlockID parent, bool includeParent)
        {
            var ret = orig(parent, includeParent);
            int count = ret.Count;

            foreach (object o in Enum.GetValues(typeof(MultiplayerUnlocks.SandboxUnlockID))) {
                var id = (MultiplayerUnlocks.SandboxUnlockID)o;

                if (MultiplayerUnlocks.ParentSandboxID(id) == parent) {
                    ret.Add(id);
                }
            }

            if (count == ret.Count) {
                // If nothing changed, return the original list.
                return ret;
            } else {
                // Otherwise, remove any duplicate elements, then return as usual.
                return ret.Distinct().ToList();
            }
        }

        private bool GetCollected(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SandboxUnlockID orig, MiscProgressionData self, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            if (unlockID == MultiplayerUnlocks.SandboxUnlockID.Slugcat) {
                return true;
            }
            if (sboxes.Values.Any(sbox => sbox.SandboxUnlocks.Any(u => u.Type == unlockID))) {
                return FisobSave.CurrentSlot.Unlocked.Contains(unlockID.ToString());
            }
            return orig(self, unlockID);
        }

        private bool SetCollected(On.PlayerProgression.MiscProgressionData.orig_SetTokenCollected_SandboxUnlockID orig, MiscProgressionData self, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            if (sboxes.Values.Any(sbox => sbox.SandboxUnlocks.Any(u => u.Type == unlockID))) {
                bool alreadySet = FisobSave.CurrentSlot.Unlocked.Contains(unlockID.ToString());
                if (alreadySet) {
                    return false;
                }
                FisobSave.Unlock(unlockID.ToString());
                return true;
            }
            return orig(self, unlockID);
        }
    }
}
