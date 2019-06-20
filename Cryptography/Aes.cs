using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ready.Framework.Cryptography
{
    public static class Aes
    {
        private const string Salt = "dtdsgnmgmt";
        private const string HashAlgorithm = "SHA-512";
        private const string Vector = "GrTy21?!aAsD*L0b";
        private const string Password = "dcYyfYRkA6Qd7uekRD2y";
        public static string Encrypt(string plainText, string password = Password, string salt = Salt,
            string hashAlgorithm = HashAlgorithm, int passwordIterations = 2, string initialVector = Vector,
            int keySize = 256)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            var initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(salt);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm, passwordIterations);
            var keyBytes = derivedPassword.GetBytes(keySize / 8);

            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            byte[] cipherTextBytes;
            using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes))
            {
                using (var memStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }

            symmetricKey.Clear();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cipherText, string password = Password, string salt = Salt,
            string hashAlgorithm = HashAlgorithm, int passwordIterations = 2, string initialVector = Vector,
            int keySize = 256)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            var initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(Salt);
            var cipherTextBytes = Convert.FromBase64String(cipherText);

            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm, passwordIterations);
            var keyBytes = derivedPassword.GetBytes(keySize / 8);

            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            var plainTextBytes = new byte[cipherTextBytes.Length];
            var byteCount = 0;

            using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
            {
                using (var memStream = new MemoryStream(cipherTextBytes))
                {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                    {
                        byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }
    }
}
