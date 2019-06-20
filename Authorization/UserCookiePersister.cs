//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using System;

//namespace Ready.Framework.Authorization
//{
//    internal static class UserCookiePersister<TUser>
//        where TUser : class, IUser
//    {
//        public static void Persist(TUser state)
//        {
//            var serializedUserState = JsonConvert.SerializeObject(state);
//            var ticket = new FormsAuthenticationTicket(1, state.Id.ToString(), DateTime.Now, DateTime.Now.AddYears(1),
//                false, serializedUserState, FormsAuthentication.FormsCookiePath);

//            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
//            if (encryptedTicket != null)
//                CookieManager.Add(CookieManager.LoginCookieName, encryptedTicket);
//        }

//        public static TUser GetState()
//        {
//            var contextUserState = HttpContext.Current.Items["UserState"];
//            if (contextUserState != null)
//                return (TUser) contextUserState;

//            var cookies = CookieManager.GetValueCollection();
//            if (cookies == null) return null;

//            TUser userState = null;
//            var cookie = cookies[CookieManager.LoginCookieName];

//            // Eski sürümdeki cookie'lere destek verebilmek için.
//            if (cookie == null)
//                cookie = cookies[FormsAuthentication.FormsCookieName];

//            if (cookie != null)
//            {
//                var ticket = FormsAuthentication.Decrypt(cookie);

//                if (ticket != null)
//                    userState = JsonConvert.DeserializeObject<TUser>(ticket.UserData);
//            }

//            HttpContext.Current.Items["UserState"] = userState;

//            return userState;
//        }

//        public static void ClearCache()
//        {
//            HttpContext.Current.Items["UserState"] = null;
//        }
//    }
//}