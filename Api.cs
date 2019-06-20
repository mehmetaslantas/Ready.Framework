using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ready.Framework.Authorization;
using Ready.Framework.Configuration;
using Ready.Framework.Extensions;
using Ready.Framework.Infrastructure;
using Ready.Framework.Logging;
using Ready.Framework.Service.Base;
using Ready.Framework.Versioning;

namespace Ready.Framework
{
    public sealed class Api<TFacade, TClient, TUser>
        where TFacade : class, IServiceVersionFacade
        where TClient : class, IClient
        where TUser : class, IUser
    {
        // Generic Class içinde Static Field verildi çünkü her bir T için farklı bir Static API olması isteniyor
        private static volatile Api<TFacade, TClient, TUser> _api;

        // Generic Class içinde Static Field verildi çünkü her bir T için Lock işlemlerinin kendi objesini kullanması isteniyor
        // ReSharper disable once StaticMemberInGenericType
        private static volatile object _lockObject = new object();

        private static IAuthProvider<TClient, TUser> _authProvider;

        private Api(ref IAuthProvider<TClient, TUser> authProvider)
        {
            _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));

            var modules = Host.Current.ServiceModules.Where(facade => facade is TFacade).OrderBy(facade => facade.Version?.Replace(".", string.Empty).ToInt32());

            foreach (var serviceFacade in modules)
                AddServiceFacade((TFacade)serviceFacade, ref authProvider);
        }

        private List<TFacade> ServiceFacades { get; } = new List<TFacade>();

        public TFacade Services
        {
            get
            {
                if (ServiceFacades.Count == 0)
                    throw new ArgumentException($"There is no ServiceVersionFacade");

                return ServiceFacades.FirstOrDefault(facade => facade.IsEnable(_authProvider.GetClient()?.ClientVersion ?? string.Empty)) ?? ServiceFacades.Last();
            }
        }

        public TFacade SpecificFacade(string version)
        {
            if (ServiceFacades.Count == 0)
                throw new ArgumentException($"There is no ServiceVersionFacade");

            return ServiceFacades.FirstOrDefault(x => x.Version == version);
        }

        public static Api<TFacade, TClient, TUser> GetApi(ref IAuthProvider<TClient, TUser> authProvider)
        {
            if (_api != null)
                return _api;

            lock (_lockObject)
            {
                if (_api == null)
                    _api = new Api<TFacade, TClient, TUser>(ref authProvider);
            }

            return _api;
        }

        public void AddServiceFacade(TFacade serviceFacade, ref IAuthProvider<TClient, TUser> authProvider)
        {
            var decoratedProxy = GetDecoratedProxy(serviceFacade, ref authProvider);
            ServiceFacades.Add(decoratedProxy);
        }

        public static void AddConfigurationService(IConfigurationService configurationService)
        {
            ConfigurationManager.AddConfigurationService(configurationService);
        }

        public static void AddServiceLogger(IServiceLogger<TClient, TUser> serviceLogger)
        {
            ServiceLogManager<TClient, TUser>.AddLogService(serviceLogger);
        }

        private TFacade GetDecoratedProxy(TFacade facade, ref IAuthProvider<TClient, TUser> authProvider)
        {
            var propertyInfos = facade.GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.PropertyType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<,,>)))
                .ToList();

            foreach (var propertyInfo in propertyInfos)
            {
                var currentService = propertyInfo.GetValue(facade);
                var currentServiceInterface = currentService.GetType().GetInterfaces()
                    .FirstOrDefault(i => i.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<,,>)));

                var proxiedServiceType = typeof(ProxyService<,,,>).MakeGenericType(typeof(TFacade), currentServiceInterface, typeof(TClient), typeof(TUser));
                var args = new[] { currentService, facade.Version };

                var proxiedService = (IProxyService<TClient, TUser>)proxiedServiceType.InvokeMember("Create", BindingFlags.InvokeMethod, null, null, args);
                proxiedService.SetParameters(ref authProvider);

                propertyInfo.SetValue(facade, proxiedService);
            }

            return facade;
        }

        public IAuthProvider<TClient, TUser> GetAuthProvider()
        {
            return _authProvider;
        }
    }
}