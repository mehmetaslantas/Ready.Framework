using System;
using System.Reflection;
using Ready.Framework.Authorization;

namespace Ready.Framework.Logging
{
    public interface IServiceLogger<in TClient, in TUser>
        where TClient : class, IClient
        where TUser : class, IUser
    {
        void OnBeforeMethodCall(TClient client, TUser user, MethodInfo targetMethodInfo, object[] args, bool isGetFromCache);

        void OnAfterMethodCall(TClient client, TUser user, MethodInfo targetMethodInfo, object[] args, object result, bool isGetFromCache);

        void OnException(TClient client, TUser user, Exception ex, MethodInfo targetMethodInfo);
    }
}