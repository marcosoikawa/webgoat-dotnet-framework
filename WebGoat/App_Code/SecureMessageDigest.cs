using System;
using System.Security.Cryptography;
using System.Text;
using log4net;
using System.Reflection;

namespace OWASP.WebGoat.NET.App_Code
{
    // SECURE VERSION: Cryptographically secure message digest
    public class SecureMessageDigest
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Generates a secure SHA-256 hash of the input message
        /// </summary>
        /// <param name="msg">The message to hash</param>
        /// <returns>Base64 encoded hash</returns>
        public static string GenerateSecureHash(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                throw new ArgumentException("Message cannot be null or empty", nameof(msg));

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(msg);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                
                log.Debug($"Generated secure hash for message length: {msg.Length}");
                
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Generates a secure SHA-256 hash with salt
        /// </summary>
        /// <param name="msg">The message to hash</param>
        /// <param name="salt">The salt to use</param>
        /// <returns>Base64 encoded hash</returns>
        public static string GenerateSecureHashWithSalt(string msg, string salt)
        {
            if (string.IsNullOrEmpty(msg))
                throw new ArgumentException("Message cannot be null or empty", nameof(msg));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            using (SHA256 sha256 = SHA256.Create())
            {
                string saltedMessage = msg + salt;
                byte[] inputBytes = Encoding.UTF8.GetBytes(saltedMessage);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                
                log.Debug($"Generated secure salted hash for message length: {msg.Length}");
                
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Generates a secure SHA-256 hash with random salt
        /// </summary>
        /// <param name="msg">The message to hash</param>
        /// <param name="salt">Output parameter for the generated salt</param>
        /// <returns>Base64 encoded hash</returns>
        public static string GenerateSecureHashWithRandomSalt(string msg, out string salt)
        {
            if (string.IsNullOrEmpty(msg))
                throw new ArgumentException("Message cannot be null or empty", nameof(msg));

            // Generate random salt
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[16]; // 128-bit salt
                rng.GetBytes(saltBytes);
                salt = Convert.ToBase64String(saltBytes);
            }

            return GenerateSecureHashWithSalt(msg, salt);
        }

        /// <summary>
        /// Verifies a message against a hash and salt
        /// </summary>
        /// <param name="msg">The message to verify</param>
        /// <param name="hash">The hash to verify against</param>
        /// <param name="salt">The salt used in the original hash</param>
        /// <returns>True if the message matches the hash</returns>
        public static bool VerifyHash(string msg, string hash, string salt)
        {
            if (string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                string computedHash = string.IsNullOrEmpty(salt) 
                    ? GenerateSecureHash(msg)
                    : GenerateSecureHashWithSalt(msg, salt);
                
                // Use constant-time comparison to prevent timing attacks
                return ConstantTimeEquals(hash, computedHash);
            }
            catch (Exception ex)
            {
                log.Error("Error verifying hash", ex);
                return false;
            }
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <returns>True if strings are equal</returns>
        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}