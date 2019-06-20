//using System;

//namespace Ready.Framework.Authorization
//{
//    public sealed class DefaultAuthProvider<TClient, TUser> : IAuthProvider<TClient, TUser>
//        where TClient : class, IClient
//        where TUser : class, IUser
//    {
//        public TClient GetClient()
//        {
//            try
//            {
//                return ClientCookiePersister<TClient>.GetState();
//            }
//            catch (Exception ex)
//            {
//                return default(TClient);
//            }   
//        }

//        public TUser GetUser()
//        {
//            try
//            {
//                return UserCookiePersister<TUser>.GetState();
//            }
//            catch (Exception ex)
//            {
//                return default(TUser);
//            }
//        }

//        public void SetClient(TClient client)
//        {
//            ClientCookiePersister<TClient>.Persist(client);
//        }

//        public void SetUser(TUser user)
//        {
//            UserCookiePersister<TUser>.Persist(user);
//        }

//        public static void ClearCache()
//        {
//            ClientCookiePersister<TClient>.ClearCache();
//            UserCookiePersister<TUser>.ClearCache();
//        }
//    }
//}