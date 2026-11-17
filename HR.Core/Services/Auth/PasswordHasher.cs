using System.Security.Cryptography;

namespace HR.Services.Auth
{
    public readonly record struct PasswordHash(byte[] Hash, byte[] Salt, int Iterations);

    /// <summary>PBKDF2 با SHA-256، نمک جدا برای هر کاربر، و تعداد تکرار ذخیره‌شده.</summary>
    public static class PasswordHasher
    {
        public const int SaltSize = 16;
        public const int HashSize = 32;
        public const int DefaultIterations = 100_000;

        public static PasswordHash Create(string password, int? iterations = null)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("رمز عبور خالی است.", nameof(password));

            var rounds = iterations ?? DefaultIterations;
            if (rounds < 10_000)
                throw new ArgumentOutOfRangeException(nameof(iterations), "تعداد تکرار کافی نیست.");

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, rounds, HashAlgorithmName.SHA256, HashSize);
            return new PasswordHash(hash, salt, rounds);
        }

        public static bool Verify(string? password, byte[]? hash, byte[]? salt, int iterations)
        {
            if (string.IsNullOrEmpty(password) || hash is not { Length: > 0 } || salt is not { Length: > 0 } || iterations < 1)
                return false;

            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
            return CryptographicOperations.FixedTimeEquals(actual, hash);
        }
    }
}
