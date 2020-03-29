using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BTCLibAsync
{
    public static class Helpers
    {
        public static Task<string> ByteArrayToString(byte[] ba)
        {
            return Task.FromResult(BitConverter.ToString(ba).Replace("-", "").ToLower());
        }

        public static Task<byte[]> StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return Task.FromResult(bytes);
        }
    }
}
