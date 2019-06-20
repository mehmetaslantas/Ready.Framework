//using System.Web;
//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using Ready.Framework.Configuration;
//using Ready.Framework.Cryptography;

//namespace Ready.Framework.Authorization
//{
//    internal static class ClientCookiePersister<TClient>
//    {
//        private static GZipCompression Compresser => new GZipCompression();
//        private static RijndaelCryptography CryptoProvider => new RijndaelCryptography();
//        private static string InitCookieName => CookieManager.InitCookieName;

//        public static void Persist(TClient state)
//        {
//            var zippedData = Compresser.Zip(JsonConvert.SerializeObject(state));
//            var serializedData = JsonConvert.SerializeObject(zippedData);

//            var encryptedData = CryptoProvider.Encrypt(serializedData);
//            CookieManager.Add(InitCookieName, HttpUtility.UrlEncode(encryptedData));
//        }

//        public static TClient GetState()
//        {
//            var contextClientState = HttpContext.Current.Items["ClientState"];
//            if (contextClientState != null)
//                return (TClient) contextClientState;

//            var cookie = CookieManager.GetValueCollection();
//            if (cookie == null)
//                return default(TClient);

//            var clientState = default(TClient);

//            var value = cookie[InitCookieName];
//            if (!string.IsNullOrEmpty(value))
//            {
//                var decryptedData = CryptoProvider.Decrypt(HttpUtility.UrlDecode(value));
//                var deserializedData = JsonConvert.DeserializeObject<byte[]>(decryptedData);
//                clientState = JsonConvert.DeserializeObject<TClient>(Compresser.Unzip(deserializedData));
//            }

//            HttpContext.Current.Items["ClientState"] = clientState;

//            return clientState;
//        }

//        public static void ClearCache()
//        {
//            HttpContext.Current.Items["ClientState"] = null;
//        }
//    }
//}