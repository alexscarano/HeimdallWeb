using System.Security.Cryptography;

namespace HeimdallWeb.Helpers
{
    public static class PasswordUtils
    {
        public static string hashPassword(this string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // PBKDF2 com 100.000 iterações
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // Guardar salt + hash (em Base64)
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hashToCompare = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
        }
    }
}
}
