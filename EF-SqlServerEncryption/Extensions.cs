using System.Collections.Generic;
using System.Linq;

namespace EF_SqlServerEncryption {
    public static class Extensions {
        public static string ToHexadecimalString(this IEnumerable<byte> bytes) {
            return "0x" + string.Concat(bytes.Select(b => b.ToString("X2")));
        }

        public static byte[] ToByteArray(this string str) {
            var bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}