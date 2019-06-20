using System;
using System.Collections.Generic;
using Ready.Framework.Attributes;
using Ready.Framework.Caching.Models;
using Ready.Framework.Caching.Models.Enum;
using Ready.Framework.Caching.Models.Interfaces;
using Ready.Framework.Configuration;

namespace Ready.Framework.Caching
{
    public static class CacheManager
    {
        private static volatile object _locker = new object();

        private static ICache CachingContext { get; set; }

        private static ICache BackupCacheContext { get; set; }

        private static DateTime? MasterCacheFallDownTime { get; set; }

        public static CachingType CachingType { get; } = ConfigurationManager.GetParameter("CachingType", CachingType.Runtime);

        public static CachingType BackupCachingType { get; } = ConfigurationManager.GetParameter("BackupCachingType", CachingType.Runtime);

        public static string CachingPrefix { get; } = $"{ConfigurationManager.GetParameter("CachePrefix", "-")}{ConfigurationManager.ApplicationEnvironment}-";

        public static bool IgnorePrefix { get; } = !Current.UsePrefix || ConfigurationManager.GetParameter("IgnorePrefix", false) || string.IsNullOrEmpty(CachingPrefix);

        public static int DefaultCacheDuration { get; } = ConfigurationManager.GetParameter("DefaultCacheDuration", 1440);

        public static bool CachingEnabled => ConfigurationManager.GetParameter("CachingEnabled", true);

        private static ICache Current
        {
            get
            {
                if (CachingContext != null)
                    return CachingContext;

                lock (_locker)
                {
                    if (CachingContext == null)
                        switch (CachingType)
                        {
                            case CachingType.RedisWithRuntime:
                                CachingContext = new RedisWithRuntimeCache();
                                break;
                            case CachingType.Runtime:
                                CachingContext = new RuntimeCache();
                                break;
                        }
                }

                return CachingContext;
            }
        }

        private static ICache BackUpCache
        {
            get
            {
                if (BackupCacheContext != null)
                    return BackupCacheContext;

                lock (_locker)
                {
                    if (BackupCacheContext == null)
                        switch (BackupCachingType)
                        {
                            case CachingType.RedisWithRuntime:
                                BackupCacheContext = new RedisWithRuntimeCache();
                                break;
                            case CachingType.Runtime:
                                BackupCacheContext = new RuntimeCache();
                                break;
                        }
                }

                return BackupCacheContext;
            }
        }


        public static ICache UseBackupConnection(Exception ex)
        {
            MasterCacheFallDownTime = DateTime.Now;
            return BackUpCache;
        }

        public static object Get(string key)
        {
            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            return Current.Get(key);
        }

        public static T Get<T>(string key)
        {
            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            return Current.Get<T>(key);
        }

        public static bool Get<T>(string key, out T value) where T : class
        {
            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            return Current.TryGet(key, out value);
        }

        public static T Get<T>(string keyGroup, string key)
        {
            if (Current.CachingFormat == CachingFormat.String && Current.NeedsSerialization)
                return Get<T>(keyGroup + "-" + key);

            return (T)Get(keyGroup, key);
        }

        public static object Get(string keyGroup, string key)
        {
            object returnValue = null;
            if (Current.CachingFormat == CachingFormat.Binary || !Current.NeedsSerialization)
            {
                var cacheGroup = Get<Dictionary<string, object>>(keyGroup);
                cacheGroup?.TryGetValue(key, out returnValue);
            }

            return returnValue;
        }

        public static void Set(string key, object value, CachingAttribute.CachingProfiles cacheProfile)
        {
            Set(key, value, (int)cacheProfile);
        }

        public static void Set(string key, object value, int duration = 0)
        {
            if (!CachingEnabled)
                return;

            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            if (duration <= 0)
                duration = DefaultCacheDuration;

            Current.Set(key, value, duration);
        }

        public static void Set(string key, object value, DateTime absoluteExpiration)
        {
            if (!CachingEnabled)
                return;

            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            if (absoluteExpiration <= DateTime.MinValue)
                absoluteExpiration = DateTime.Now.AddMinutes(DefaultCacheDuration);

            Current.Set(key, value, absoluteExpiration);
        }

        public static void Set(string keyGroup, string key, object value, CachingAttribute.CachingProfiles cacheProfile)
        {
            Set(keyGroup, key, value, (int)cacheProfile);
        }

        public static void Set(string keyGroup, string key, object value, int duration)
        {
            Set(keyGroup, key, value, DateTime.Now.AddMinutes(duration));
        }

        public static void Set(string keyGroup, string key, object value, DateTime absoluteExpiration)
        {
            if (!CachingEnabled)
                return;

            if (Current.CachingFormat == CachingFormat.String && Current.NeedsSerialization)
            {
                Set(keyGroup + "-" + key, value, absoluteExpiration);
            }
            else
            {
                if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !keyGroup.StartsWith(CachingPrefix))
                    keyGroup = CachingPrefix + keyGroup;

                var cacheGroup = Get<Dictionary<string, object>>(keyGroup) ?? new Dictionary<string, object>();

                if (cacheGroup.TryGetValue(key, out _))
                    cacheGroup[key] = value;
                else
                    cacheGroup.Add(key, value);

                Set(keyGroup, cacheGroup, absoluteExpiration);
            }
        }

        public static bool IsExists(string key)
        {
            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            return Current.IsExists(key);
        }

        public static void Remove(string key)
        {
            if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !key.StartsWith(CachingPrefix))
                key = CachingPrefix + key;

            Current.Remove(key);
        }

        public static void Remove(string keyGroup, string key)
        {
            if (Current.CachingFormat == CachingFormat.String && Current.NeedsSerialization)
            {
                Remove(keyGroup + "-" + key);
            }
            else
            {
                if (!IgnorePrefix && !string.IsNullOrEmpty(CachingPrefix) && !keyGroup.StartsWith(CachingPrefix))
                    keyGroup = CachingPrefix + keyGroup;

                var cacheGroup = Get<Dictionary<string, object>>(keyGroup);
                cacheGroup?.Remove(key);
            }
        }

        public static bool RemoveAll(string prefix = "")
        {
            if (!Current.UsePrefix)
                prefix = string.Empty;

            return Current.RemoveAll(prefix);
        }

        public static IEnumerable<string> GetKeys(string prefix = "")
        {
            return Current.GetKeys(prefix);
        }
    }
}