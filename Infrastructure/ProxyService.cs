using System;
using System.Linq;
using System.Reflection;
using Ready.Framework.Attributes;
using Ready.Framework.Authorization;
using Ready.Framework.Caching;
using Ready.Framework.Extensions;
using Ready.Framework.Logging;
using Ready.Framework.Service.Base;
using Ready.Framework.Service.Models;
using Ready.Framework.Versioning;

#pragma warning disable 168

namespace Ready.Framework.Infrastructure
{
    public class ProxyService<TServiceFacade, TService, TClient, TUser> : DispatchProxy, IProxyService<TClient, TUser>
        where TServiceFacade : IServiceVersionFacade
        where TService : class, IService<TServiceFacade, TClient, TUser>
        where TClient : class, IClient
        where TUser : class, IUser
    {
        private IAuthProvider<TClient, TUser> _authProvider;
        private TService _decorated;

        public ProxyService()
        {
        }

        private ProxyService(TService decorated, string serviceVersion)
        {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            ServiceVersion = serviceVersion;
        }

        private string ServiceVersion { get; set; }

        protected TClient Client
        {
            get
            {
                try
                {
                    return _authProvider.GetClient();
                }
                catch (Exception ex)
                {
                    return default(TClient);
                }
            }
        }

        protected TUser User
        {
            get
            {
                try
                {
                    return _authProvider.GetUser();
                }
                catch (Exception ex)
                {
                    return default(TUser);
                }
            }
        }

        public void SetParameters(ref IAuthProvider<TClient, TUser> authProvider)
        {
            _authProvider = authProvider;
            _decorated.AuthProvider = _authProvider;
        }

        public static ProxyService<TServiceFacade, TService, TClient, TUser> Create(TService decorated, string serviceVersion)
        {
            object proxiedService = Create<TService, ProxyService<TServiceFacade, TService, TClient, TUser>>();

            var proxy = (ProxyService<TServiceFacade, TService, TClient, TUser>)proxiedService;
            proxy._decorated = decorated;
            proxy.ServiceVersion = serviceVersion;

            return proxy;
        }

        private MethodInfo GetSourceTargetMethod(MethodInfo targetMethod)
        {
            var sourceTargetMethod = targetMethod;

            try
            {
                sourceTargetMethod = _decorated.GetType().GetMethod(targetMethod.Name, targetMethod.GetParameters().Select(x => x.ParameterType).ToArray());
            }
            catch (Exception ex)
            {
                // Eğer ambigous exception veya farklı bir reflection hatası verirse devam etsin    
            }

            if (sourceTargetMethod == null)
                sourceTargetMethod = targetMethod;

            return sourceTargetMethod;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            object result = null;
            var sourceTargetMethod = GetSourceTargetMethod(targetMethod);

            try
            {
                if (targetMethod == null || sourceTargetMethod == null)
                    throw new ArgumentException($"Middleware Framework could not get {nameof(targetMethod)}");

                var cacheKey = string.Empty;
                var isGetFromCache = false;

                var cachingProfile = sourceTargetMethod.GetCustomAttribute<CachingAttribute>();
                var isCaching = sourceTargetMethod.IsCaching();

                if (isCaching)
                {
                    cacheKey = sourceTargetMethod.GenerateCacheKey(args, Client.GetCachePrefix(ServiceVersion));

                    var cachedData = CacheManager.Get(cacheKey);

                    if (cachedData != null)
                    {
                        isGetFromCache = true;
                        if (!sourceTargetMethod.HasServiceResponse())
                        {
                            result = cachedData;
                        }
                        else
                        {
                            result = Activator.CreateInstance(sourceTargetMethod.ReturnType);

                            var propertyInfo = sourceTargetMethod.ReturnType.GetProperty("Data");
                            propertyInfo?.SetValue(result, cachedData);
                        }
                    }
                }

                ServiceLogManager<TClient, TUser>.OnBeforeMethodCall(Client, User, sourceTargetMethod, args, isGetFromCache);

                if (!isGetFromCache)
                {
                    try
                    {
                        result = sourceTargetMethod.Invoke(_decorated, args);
                    }
                    catch (Exception ex)
                    {
                        ServiceLogManager<TClient, TUser>.OnException(Client, User, ex, sourceTargetMethod);

                        if (sourceTargetMethod.HasServiceResponse())
                            result = ex.ToMessageResponse(sourceTargetMethod);
                        else
                            throw;
                    }

                    sourceTargetMethod.CheckDropCache();

                    if (isCaching && result != null)
                    {
                        var cacheData = result;

                        if (sourceTargetMethod.HasGenericServiceResponse())
                        {
                            var propertyInfo = sourceTargetMethod.ReturnType.GetProperty("Data");
                            cacheData = propertyInfo?.GetValue(result);
                        }

                        CacheManager.Set(cacheKey, cacheData, cachingProfile.CacheDuration);
                    }
                }

                if (sourceTargetMethod.HasGenericServiceResponse())
                {
                    var propertyInfo = sourceTargetMethod.ReturnType.GetProperty(nameof(ServiceResponse.HasAuth));
                    var hasAuth = (bool)(propertyInfo?.GetValue(result) ?? false);
                    if (!hasAuth)
                        propertyInfo?.SetValue(result, User != null);
                }

                ServiceLogManager<TClient, TUser>.OnAfterMethodCall(Client, User, sourceTargetMethod, args, result, isGetFromCache);

                return result;
            }
            catch (Exception ex)
            {
                var targetException = ex.InnerException ?? ex;

                ServiceLogManager<TClient, TUser>.OnException(Client, User, targetException, sourceTargetMethod);

                throw targetException;
            }
        }
    }
}