using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ready.Framework.Cryptography
{
    public static class SymmetricAlgorithmCryptography
    {
        private const int Iterations = 2;
        private const int KeySize = 256;

        private const string Hash = "SHA-512";
        private const string Salt = "fqkvxfavqf413jce0g";
        private const string Vector = "w83euktaj2zdn9gcc7";

        public static string Encrypt<T>(string plainText, string password) where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = Encoding.ASCII.GetBytes(Vector);
            var saltBytes = Encoding.ASCII.GetBytes(Salt);
            var valueBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] encrypted;
            using (var cipher = new T())
            {
                var passwordBytes = new PasswordDeriveBytes(password, saltBytes, Hash, Iterations);
                var keyBytes = passwordBytes.GetBytes(KeySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (var encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                {
                    using (var to = new MemoryStream())
                    {
                        using (var writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                cipher.Clear();
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt<T>(string cipherText, string password) where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = Encoding.ASCII.GetBytes(Vector);
            var saltBytes = Encoding.ASCII.GetBytes(Salt);
            var valueBytes = Convert.FromBase64String(cipherText);

            byte[] decrypted;
            int decryptedByteCount;

            using (var cipher = new T())
            {
                var passwordBytes = new PasswordDeriveBytes(password, saltBytes, Hash, Iterations);
                var keyBytes = passwordBytes.GetBytes(KeySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (var decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                    {
                        using (var from = new MemoryStream(valueBytes))
                        {
                            using (var reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[valueBytes.Length];
                                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    }
                }
                catch
                {
                    return string.Empty;
                }

                cipher.Clear();
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
    }
}