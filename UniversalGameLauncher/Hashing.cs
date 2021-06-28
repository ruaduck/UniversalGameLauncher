using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UniversalGameLauncher
{
    class Hashing
    {
        // The cryptographic service provider.
        private static SHA256 Sha256 = SHA256.Create();

        // Compute the file's hash.
        private static byte[] GetHashSha256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                return Sha256.ComputeHash(stream);
            }
        }
       private static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }
        public static string GetSHA256 (string filename)
        {
            return BytesToString(GetHashSha256(filename));
        }
    }
}
