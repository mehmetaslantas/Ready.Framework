//using System;
//using System.Collections.Specialized;

//namespace Ready.Framework.Authorization
//{
//    internal static class CookieManager
//    {
//        public const string DefaultCookieName = "m";

//        public static string InitCookieName => "i";

//        public static string LoginCookieName => "ui";

//        public static string DeviceUserCookieCookieName => "dui";

//        public static string CookieEncryptionKey => "beinconnectmw";

//        public static void Add(string name, string value)
//        {
//            var valueCollection = GetValueCollection();
//            if (valueCollection == null)
//            {
//                var newCookie = new HttpCookie(DefaultCookieName)
//                {
//                    Path = "/",
//                    Expires = DateTime.Now.AddYears(1)
//                };
//                newCookie.Values.Add(name, value);

//                HttpContext.Current.Response.Cookies.Add(newCookie);
//            }
//            else
//            {
//                var newCookie = new HttpCookie(DefaultCookieName)
//                {
//                    Path = "/",
//                    Expires = DateTime.Now.AddYears(1)
//                };

//                valueCollection.Set(name, value);

//                var responseCookie = HttpContext.Current.Response.Cookies[DefaultCookieName];
//                if (responseCookie != null && responseCookie.Values.Count > 0)
//                    responseCookie.Values.AllKeys.ToList().ForEach(key => valueCollection.Set(key, responseCookie.Values[key]));

//                newCookie.Values.Add(valueCollection);

//                HttpContext.Current.Response.Cookies.Set(newCookie);
//            }
//        }

//        public static NameValueCollection GetValueCollection()
//        {
//            try
//            {
//                NameValueCollection values = null;

//                var cookie = HttpContext.Current.Request.Cookies[DefaultCookieName];
//                if (cookie != null)
//                    values = cookie.Values;

//                var requestCustomCookieHeader = HttpContext.Current.Request.Headers["X-Cookie"];
//                if (!string.IsNullOrEmpty(requestCustomCookieHeader))
//                {
//                    var defaultCookieStartIndex = requestCustomCookieHeader.IndexOf($"{DefaultCookieName}=", StringComparison.InvariantCultureIgnoreCase) + DefaultCookieName.Length + 1;

//                    var defaultCookieEndIndex = requestCustomCookieHeader.IndexOf(";", defaultCookieStartIndex, StringComparison.InvariantCultureIgnoreCase);
//                    if (defaultCookieEndIndex < 0)
//                        defaultCookieEndIndex = requestCustomCookieHeader.Length;

//                    var strCookie = requestCustomCookieHeader.Substring(defaultCookieStartIndex, defaultCookieEndIndex - defaultCookieStartIndex);
//                    var customCookie = new HttpCookie(DefaultCookieName, strCookie);
//                    values = customCookie.Values;
//                }

//                var requestAuthorizationHeader = HttpContext.Current.Request.Headers["Authorization"]?.ReplaceFirst("Bearer ", "");
//                if (!string.IsNullOrEmpty(requestAuthorizationHeader))
//                {
//                    var strCookie = requestAuthorizationHeader;
//                    var customCookie = new HttpCookie(DefaultCookieName, strCookie);
//                    values = customCookie.Values;
//                }

//                return values;
//            }
//            catch (Exception)
//            {
//                return null;
//            }
//        }

//        public static void Clear()
//        {
//            HttpContext.Current.Response.Cookies.Clear();
//        }
//    }
//}