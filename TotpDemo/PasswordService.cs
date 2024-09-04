using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace TotpDemo
{
    public class PasswordService
    {
        private readonly IConfiguration _configuration;
        private readonly string _staticSalt;

        public PasswordService(IConfiguration configuration)
        {
            _configuration = configuration;
            _staticSalt = _configuration["Salt"] ?? throw new InvalidOperationException("Salt not found in configuration");
        }

        /// <summary>
        /// Hashes the entered passwprd with the static salt.
        /// </summary>
        public string HashPassword(string password)
        {
            var saltedPassword = password + _staticSalt;

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hash);
            }
        }

        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var hashOfEnteredPassword = HashPassword(enteredPassword);

            return hashOfEnteredPassword == storedHash;
        }
    }
}
