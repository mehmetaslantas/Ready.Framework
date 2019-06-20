using System;
using System.Security.Cryptography;
using System.Text;
using Ready.Framework.Configuration;
using Ready.Framework.Extensions;

namespace Ready.Framework.Cryptography
{
    public static class CryptoManager
    {
        public static string Encrypt(string chipperText, string encryptionKey = "")
        {
            try
            {
                if (string.IsNullOrEmpty(chipperText))
                    return chipperText;

                if (string.IsNullOrEmpty(encryptionKey))
                    encryptionKey = ConfigurationManager.EncryptKey;

                var cryptoProvider = new TripleDESCryptoServiceProvider
                {
                    Key = encryptionKey.ToByteArray(),
                    Mode = CipherMode.ECB
                };

                var encryptor = cryptoProvider.CreateEncryptor();
                var buffer = Encoding.ASCII.GetBytes(chipperText);

                return Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch
            {
                return chipperText;
            }
        }

        public static string Decrypt(string richText, string decryptionKey = "")
        {
            try
            {
                if (string.IsNullOrEmpty(richText))
                    return richText;

                if (string.IsNullOrEmpty(decryptionKey))
                    decryptionKey = ConfigurationManager.EncryptKey;

                var cryptoProvider = new TripleDESCryptoServiceProvider
                {
                    Key = decryptionKey.ToByteArray(),
                    Mode = CipherMode.ECB
                };

                var decryptor = cryptoProvider.CreateDecryptor();
                var buffer = Convert.FromBase64String(richText);

                return Encoding.ASCII.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch
            {
                return richText;
            }
        }

        public static string ComputeHashByAlgorithm(string plainText, string saltText = "", HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding encoding = null, bool useBase64String = true)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return plainText;
                if (encoding == null)
                    encoding = Encoding.Unicode;
                HashAlgorithm hashProvider = null;
                switch (algorithm)
                {
                    default:
                    case HashAlgorithms.SHA1:
                        hashProvider = new SHA1CryptoServiceProvider();
                        break;
                    case HashAlgorithms.SHA256:
                        hashProvider = new SHA256CryptoServiceProvider();
                        break;
                    case HashAlgorithms.SHA384:
                        hashProvider = new SHA384CryptoServiceProvider();
                        break;
                    case HashAlgorithms.SHA512:
                        hashProvider = new SHA512CryptoServiceProvider();
                        break;
                    case HashAlgorithms.SHA512Managed:
                        hashProvider = new SHA512Managed();
                        break;
                    case HashAlgorithms.MD5:
                        hashProvider = new MD5CryptoServiceProvider();
                        break;
                }

                var dataText = plainText + saltText;
                var dataBytes = encoding.GetBytes(dataText);
                var hashedBytes = hashProvider.ComputeHash(dataBytes);
                hashProvider.Clear();
                if (useBase64String)
                    return Convert.ToBase64String(hashedBytes);
                var sBuilder = new StringBuilder();
                for (var i = 0; i < hashedBytes.Length; i++) sBuilder.Append(hashedBytes[i].ToString("x2"));
                return sBuilder.ToString();
            }
            catch
            {
                return plainText;
            }
        }

        public static string ComputeHMACHashByAlgorithm(string key, string plainText, string saltText = "", HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding encoding = null,
            bool useBase64String = false)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return plainText;
                if (string.IsNullOrEmpty(key))
                    key = Utility.GenerateRandomPassword(8);
                if (encoding == null)
                    encoding = Encoding.ASCII;
                var secretKeyBytes = encoding.GetBytes(key);
                HashAlgorithm hashProvider = null;
                switch (algorithm)
                {
                    default:
                    case HashAlgorithms.SHA1:
                        hashProvider = new HMACSHA1(secretKeyBytes);
                        break;
                    case HashAlgorithms.SHA256:
                        hashProvider = new HMACSHA256(secretKeyBytes);
                        break;
                    case HashAlgorithms.SHA384:
                        hashProvider = new HMACSHA384(secretKeyBytes);
                        break;
                    case HashAlgorithms.SHA512:
                        hashProvider = new HMACSHA512(secretKeyBytes);
                        break;
                    case HashAlgorithms.MD5:
                        hashProvider = new HMACMD5(secretKeyBytes);
                        break;
                }

                var dataText = plainText + saltText;
                var dataBytes = encoding.GetBytes(dataText);
                var hashedBytes = hashProvider.ComputeHash(dataBytes);
                hashProvider.Clear();
                if (useBase64String)
                    return Convert.ToBase64String(hashedBytes);
                return string.Concat(Array.ConvertAll(hashedBytes, x => x.ToString("x2")));
            }
            catch
            {
                return plainText;
            }
        }

        public static string GetMd5Hash(string plainText, string saltText = "", Encoding encoding = null, bool useToBase64String = true)
        {
            return ComputeHashByAlgorithm(plainText, saltText, HashAlgorithms.MD5, encoding, useToBase64String);
        }

        public static string GetSha256Hash(string plainText, string saltText = "", Encoding encoding = null, bool useToBase64String = true)
        {
            return ComputeHashByAlgorithm(plainText, saltText, HashAlgorithms.SHA256, encoding, useToBase64String);
        }

        public static string ComputeHash(string plainText, string saltText = "")
        {
            return ComputeHashByAlgorithm(plainText, saltText, HashAlgorithms.SHA512);
        }

        public static string ComputeHMACHash(string key, string plainText, string saltText = "", HashAlgorithms algorithm = HashAlgorithms.SHA1)
        {
            return ComputeHMACHashByAlgorithm(key, plainText, saltText, algorithm);
        }
    }
}