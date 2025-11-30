using System.Security.Cryptography;
using System.Text;

namespace PGSA_Licence3.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plain);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
