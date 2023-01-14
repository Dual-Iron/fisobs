using System.IO;
using System.Text;

namespace Fisobs.Saves
{
    static class SaveExt
    {
        // all operations in this class are implied little-endian
        // nothing in this class is thread-safe

        static byte[] buf = new byte[128];

        public static int? ReadU16(this Stream stream)
        {
            if (stream.Read(buf, 0, 2) == 2) {
                return buf[0] + (buf[1] << 8);
            }
            return null;
        }

        public static string? ReadStr(this Stream stream)
        {
            if (stream.Read(buf, 0, 2) != 2) {
                return null;
            }

            int len = buf[0] + (buf[1] << 8);
            if (len > buf.Length) {
                buf = new byte[len];
            }

            if (stream.Read(buf, 0, len) == len) {
                return Encoding.UTF8.GetString(buf, 0, len);
            }
            return null;
        }

        public static void WriteU16(this Stream stream, int v)
        {
            buf[0] = (byte)(v & 0xff);
            buf[1] = (byte)(v >> 8);
            stream.Write(buf, 0, 2);
        }

        public static void WriteStr(this Stream stream, string v)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            stream.WriteU16(bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
