using System;
using System.Collections.Generic;
#pragma warning disable 168

namespace Ready.Framework.Configuration
{
    public static class ConfigurationManager
    {
        private static List<IConfigurationService> ConfigurationServices { get; } = new List<IConfigurationService>();

        public static string EncryptKey => GetParameter("EncryptKey");

        public static string ApplicationEnvironment => GetParameter("ApplicationEnvironment", "Live");

        public static string ConnectionString => GetParameter("ConnectionString");

        public static int HttpsPort => GetParameter("HttpsPort", 443);

        public static string MessageCodePrefix => GetParameter("MessageCodePrefix");


        public static T GetParameter<T>(string key, T defaultValue = default(T)) where T : struct, IConvertible
        {
            foreach (var service in ConfigurationServices)
                try
                {
                    var result = service.GetParameter(key, defaultValue);

                    if (result.Success)
                        return result.Parameter;
                }
                catch (Exception ex)
                {
                    // ignored
                }

            return defaultValue;
        }

        public static string GetParameter(string key, string defaultValue = "")
        {
            foreach (var service in ConfigurationServices)
                try
                {
                    var result = service.GetParameter(key, defaultValue);

                    if (result.Success)
                        return result.Parameter;
                }
                catch (Exception ex)
                {
                    // ignored
                }

            return defaultValue;
        }

        internal static void AddConfigurationService(IConfigurationService configurationService)
        {
            ConfigurationServices.Add(configurationService);
        }
    }
}