using System;
using System.Collections.Generic;
using System.Reflection;
using Ready.Framework.Attributes;
using Ready.Framework.Authorization;

namespace Ready.Framework.Logging
{
    internal class ServiceLogManager<TClient, TUser>
        where TClient : class, IClient
        where TUser : class, IUser
    {
        private static List<IServiceLogger<TClient, TUser>> LogServices { get; } = new List<IServiceLogger<TClient, TUser>>();

        internal static void OnBeforeMethodCall(TClient client, TUser user, MethodInfo targetMethodInfo, object[] args, bool isGetFromCache)
        {
            var logAttribute = targetMethodInfo.GetCustomAttribute<LogAttribute>();
            if (logAttribute != null && !logAttribute.LogEvents)
                return;

            foreach (var logService in LogServices)
                logService.OnBeforeMethodCall(client, user, targetMethodInfo, args, isGetFromCache);
        }

        internal static void OnAfterMethodCall(TClient client, TUser user, MethodInfo targetMethodInfo, object[] args, object result, bool isGetFromCache)
        {
            var logAttribute = targetMethodInfo.GetCustomAttribute<LogAttribute>();
            if (logAttribute != null && !logAttribute.LogEvents)
                return;

            foreach (var logService in LogServices)
                logService.OnAfterMethodCall(client, user, targetMethodInfo, args, result, isGetFromCache);
        }

        internal static void OnException(TClient client, TUser user, Exception ex, MethodInfo targetMethodInfo)
        {
            var logAttribute = targetMethodInfo.GetCustomAttribute<LogAttribute>();
            if (logAttribute != null && !logAttribute.LogErrors)
                return;

            foreach (var logService in LogServices)
                logService.OnException(client, user, ex, targetMethodInfo);
        }

        internal static void AddLogService(IServiceLogger<TClient, TUser> serviceLogger)
        {
            LogServices.Add(serviceLogger);
        }
    }
}