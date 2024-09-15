using System.Security.Cryptography;
using System.Text;

namespace TaskMonitor.Utils
{
    public static class HashUtil
    {
        public static string ComputePBKDF2(string value, string salt) =>
            Convert.ToBase64String(
                Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(value),
                    Encoding.UTF8.GetBytes(salt),
                    1000,
                    HashAlgorithmName.SHA512,
                    48
                )
            );
    }
}
