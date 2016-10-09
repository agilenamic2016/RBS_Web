using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RBS.DAL;
using RBS.Models;

namespace RBS.Library
{
    public static class Security
    {
        private static Context context = new Context();

        public static string GenerateSalt()
        {
            byte[] saltBytes;
            string str = String.Empty;

            // Define min and max salt sizes.
            int minSaltSize = 5;
            int maxSaltSize = 9;

            // Generate a random number for the size of the salt.
            Random random = new Random();
            int saltSize = random.Next(minSaltSize, maxSaltSize);

            // Allocate a byte array, which will hold the salt.
            saltBytes = new byte[saltSize];

            // Initialize a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            // Fill the salt with cryptographically strong byte values.
            rng.GetNonZeroBytes(saltBytes);

            str = BitConverter.ToString(saltBytes).Replace("-", "");

            return str;
        }

        public static string HashPlainText(string pwd)
        {
            string str = String.Empty;

            if (!string.IsNullOrEmpty(pwd))
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(pwd);

                using (SHA512 shaM = new SHA512Managed())
                {
                    for (int i = 0; i < context.ITERATION; i++)
                    {
                        plainTextBytes = shaM.ComputeHash(plainTextBytes);
                    }

                    str = BitConverter.ToString(plainTextBytes).Replace("-", "");                    
                }
            }

            return str;
        }

        public static string EncryptHashWithSalt(string passwordHash, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordHash);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            byte[] plainTextWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];


            // Copy plain text bytes into resulting array.
            for (int j = 0; j < passwordBytes.Length; j++)
            {
                plainTextWithSaltBytes[j] = passwordBytes[j];
            }

            // Append salt bytes to the resulting array.
            for (int k = 0; k < saltBytes.Length; k++)
            {
                plainTextWithSaltBytes[passwordBytes.Length + k] = saltBytes[k];
            }

            using (SHA512 shaM = new SHA512Managed())
            {
                plainTextWithSaltBytes = shaM.ComputeHash(plainTextWithSaltBytes);
            }

            return BitConverter.ToString(plainTextWithSaltBytes).Replace("-", "");
        }

        // Purpose: To be used when password is passed from mobile client. Password is hashed and encrypted with salt.
        public static bool IsAuthenticated(string username, string passwordFromMobile, string salt)
        {
            bool isValid = false;

            RBSContext db = new RBSContext();

            UserModel user = db.Users.FirstOrDefault(s => s.Username.Equals(username));

            if (user != null)
            {
                // Encrypt with salt
                string passwordWithSalt = EncryptHashWithSalt(user.Password, salt);

                // Compare with DB value
                if (passwordFromMobile.Equals(passwordWithSalt))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        public static bool IsSessionValid(string username, string sessionKey)
        {
            int validity = Config.SessionKeyValidity;

            bool isValid = false;

            RBSContext db = new RBSContext();

            if (!string.IsNullOrEmpty(sessionKey))
            {
                SessionModel ss = db.Sessions.FirstOrDefault(s => s.UserID.Equals(username) && s.SessionKey.Equals(sessionKey));
                DateTime ssDt = new DateTime();

                if (ss.UpdatedDate.HasValue)
                {
                    ssDt = ss.UpdatedDate.Value;
                }
                else if (ss.CreatedDate.HasValue)
                {
                    ssDt = ss.CreatedDate.Value;
                }

                ssDt = ssDt.AddSeconds(validity);

                TimeSpan span = DateTime.Now.Subtract(ssDt);
                TimeSpan validitySpan = TimeSpan.FromSeconds(validity);

                if (TimeSpan.Compare(span, validitySpan) == -1)   // -1: span is shorter than validitySpan, 0: both are equal, 1: span is longer than validitySpan
                {
                    isValid = true;
                }
            }

            return isValid;
        }
    }
}