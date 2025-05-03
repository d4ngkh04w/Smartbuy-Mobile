using System.Security.Cryptography;
using System.Text;

namespace api.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateToken(int byteLength = 64)
        {
            var randomBytes = new byte[byteLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return CleanBase64(Convert.ToBase64String(randomBytes));
            }
        }

        private static string CleanBase64(string base64String)
        {
            return base64String
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }

        public static string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToHexString(bytes).ToLower();
            }
        }

        public static bool VerifyToken(string token, string tokenHash)
        {
            string computedHash = HashToken(token);
            return computedHash == tokenHash;
        }

        public static bool IsTokenValid(DateTime expiryDate)
        {
            return DateTime.Now <= expiryDate;
        }
    }
}