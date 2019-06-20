using System.Security.Cryptography;

namespace Ready.Framework.Cryptography
{
    internal class RijndaelCryptography
    {
        private const string Password = "p3m!_u**tZ9b_DtH$e";

        public string Encrypt(string plainText)
        {
            return string.IsNullOrEmpty(plainText)
                ? string.Empty
                : SymmetricAlgorithmCryptography.Encrypt<RijndaelManaged>(plainText, Password);
        }

        public string Decrypt(string cipherText)
        {
            return string.IsNullOrEmpty(cipherText)
                ? string.Empty
                : SymmetricAlgorithmCryptography.Decrypt<RijndaelManaged>(cipherText, Password);
        }
    }
}