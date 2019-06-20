using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Ready.Framework.Caching.Models.Base;
using Ready.Framework.Caching.Models.Enum;
using Ready.Framework.Caching.Models.Interfaces;
using Ready.Framework.Configuration;
using Ready.Framework.Extensions;

namespace Ready.Framework.Caching.Models
{
    public class RedisWithRuntimeCache : ICache
    {
        private static readonly object AccessLocker = new object();

        private static volatile RedisWithRuntimeCache _current;

        private volatile RuntimeCache _runtimeCache;

        internal RedisWithRuntimeCache()
        {
            PublisherId = Guid.NewGuid().ToString();
            SubscribeToCacheUpdate();
        }

        public static RedisWithRuntimeCache Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (AccessLocker)
                {
                    if (_current == null)
                        _current = new RedisWithRuntimeCache();
                }
                return _current;
            }
        }

        public static ICache CurrentRuntimeCache
        {
            get
            {
                if (Current._runtimeCache != null)
                    return Current._runtimeCache;

                lock (AccessLocker)
                {
                    if (Current._runtimeCache == null)
                        Current._runtimeCache = new RuntimeCache();
                }
                return Current._runtimeCache;
            }
        }

        public string KeyRedisChannelNameForUpdate => CacheManager.CachingPrefix + "cache-update";

        public string PublisherId { get; }

        public bool UsePrefix => true;

        public bool NeedsSerialization => true;

        public CachingFormat CachingFormat { get; set; }

        public void Dispose()
        {
            Connection?.Dispose();
        }

        public object Get(string key)
        {
            try
            {
                var runtimeCacheObject = CurrentRuntimeCache.Get(key);
                return runtimeCacheObject ?? Db.StringGet(key).ToString().FromJson<object>();
            }
            catch (RedisConnectionException ex)
            {
                return CacheManager.UseBackupConnection(ex).Get(key);
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public T Get<T>(string key)
        {
            var model = default(T);

            try
            {
                var runtimeCacheObject = CurrentRuntimeCache.Get<T>(key);
                if (runtimeCacheObject != null)
                    return runtimeCacheObject;

                model = Db.StringGet(key).ToString().FromJson<T>();
            }
            catch (RedisConnectionException ex)
            {
                return CacheManager.UseBackupConnection(ex).Get<T>(key);
            }
            catch
            {
                // ignored
            }

            return model;
        }

        public bool TryGet<T>(string key, out T value) where T : class
        {
            try
            {
                if (CurrentRuntimeCache.TryGet(key, out value))
                    return true;

                if (!IsExists(key))
                {
                    value = default(T);
                    return false;
                }
                value = Get<T>(key);
            }
            catch (RedisConnectionException ex)
            {
                return CacheManager.UseBackupConnection(ex).TryGet(key, out value);
            }
            catch
            {
                value = default(T);
                return false;
            }
            return true;
        }

        public void Set(string key, object value, int duration)
        {
            try
            {
                Set(key, value, DateTime.Now.AddMinutes(duration));
            }
            catch
            {
                // ignored
            }
        }

        public void Set(string key, object value, DateTime absoluteExpration)
        {
            try
            {
                var valueJson = JsonConvert.SerializeObject(value);
                Db.StringSet(key, valueJson, absoluteExpration.ToUniversalTime().Subtract(DateTime.UtcNow));

                CurrentRuntimeCache.Set(key, value, absoluteExpration);

                Publish(key, value, absoluteExpration);
            }
            catch (RedisConnectionException ex)
            {
                CacheManager.UseBackupConnection(ex).Set(key, value, absoluteExpration);
            }
            catch
            {
                // ignored
            }
        }

        public bool IsExists(string key)
        {
            try
            {
                return CurrentRuntimeCache.IsExists(key) || Db.KeyExists(key);
            }
            catch (RedisConnectionException ex)
            {
                CacheManager.UseBackupConnection(ex).IsExists(key);
            }
            catch
            {
                // ignored
            }

            return false;
        }

        public void Remove(string key)
        {
            Remove(key, true);
        }

        public bool RemoveAll(string prefix = "")
        {
            try
            {
                foreach (var key in GetRedisKeys(prefix))
                    Current.Remove(key, false);

                CurrentRuntimeCache.RemoveAll(prefix);

                PublishCleanAllCache(prefix);
            }
            catch (RedisConnectionException ex)
            {
                return CacheManager.UseBackupConnection(ex).RemoveAll(prefix);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public IEnumerable<string> GetKeys(string prefix = "")
        {
            return GetRedisKeys(prefix).Select(kvp => kvp.ToString());
        }

        private void Remove(string key, bool publishCleanCache)
        {
            try
            {
                if (IsExists(key))
                    Db.KeyDelete(key);

                CurrentRuntimeCache.Remove(key);

                if (publishCleanCache)
                    PublishCleanCache(key);
            }
            catch (RedisConnectionException ex)
            {
                CacheManager.UseBackupConnection(ex).Remove(key);
            }
            catch
            {
                // ignored
            }
        }

        public IEnumerable<RedisKey> GetRedisKeys(string prefix = "")
        {
            if (string.IsNullOrEmpty(prefix))
                prefix = CacheManager.CachingPrefix;

            foreach (var endPoint in Connection.GetEndPoints())
            {
                if (endPoint.ToString().StartsWith("127.0.0"))
                    continue;

                var server = Connection.GetServer(endPoint);
                if (server == null)
                    continue;

                var pattern = prefix + "*";
                foreach (var key in server.Keys(pattern: pattern))
                    yield return key;
            }
        }

        internal void SubscribeToCacheUpdate()
        {
            var subscriber = Connection.GetSubscriber();
            subscriber.Subscribe(KeyRedisChannelNameForUpdate, (channel, message) =>
            {
                if (channel != KeyRedisChannelNameForUpdate)
                    return;

                var messageStr = (string)message;

                if (string.IsNullOrWhiteSpace(messageStr))
                    return;

                try
                {
                    var model = messageStr.FromJson<CacheUpdateModel>();

                    // Yayınlayan bu sunucuysa işlem yapmaması için
                    if (model.Publisher == PublisherId)
                        return;

                    if (model.Key.StartsWith("##remove##"))
                    {
                        var key = model.Key.ReplaceFirst("##remove##", "");
                        CurrentRuntimeCache.Remove(key);
                        return;
                    }

                    if (model.Key.StartsWith("##removeall##"))
                    {
                        var prefix = model.Key.ReplaceFirst("##removeall##", "");
                        CurrentRuntimeCache.RemoveAll(prefix);
                        return;
                    }

                    // Gelen nesnenin anlık tipini bilmediğimiz için Json dönüştürücü bunu yapamıyor
                    // Model üzerinden tip okunup, modeldeki json bu tipe göre nesneye çevriliyor.
                    var cacheItemValueJson = JObject.Parse(messageStr)["Value"].ToString();
                    var value = JsonConvert.DeserializeObject(cacheItemValueJson, model.ValueType);

                    // Rediste zaten var o yüzden sadece runtimecache'de değişecek.
                    CurrentRuntimeCache.Set(model.Key, value, model.ExpireDate);
                }
                catch
                {
                    // ignored
                }
            });
        }

        internal void Publish<T>(string key, T value, DateTime absoluteExpration)
        {
            var model = new CacheUpdateModel
            {
                Key = key,
                Value = value,
                ValueType = value.GetType(),
                Publisher = PublisherId,
                ExpireDate = absoluteExpration
            };

            var subscriber = Connection.GetSubscriber();
            var serializeObject = JsonConvert.SerializeObject(model);
            subscriber.Publish(KeyRedisChannelNameForUpdate, serializeObject);
        }

        internal void PublishCleanCache(string key)
        {
            var model = new CacheUpdateModel
            {
                Key = "##remove##" + key,
                Value = null,
                ValueType = null,
                Publisher = PublisherId,
                ExpireDate = DateTime.MinValue
            };

            var subscriber = Connection.GetSubscriber();
            var serializeObject = JsonConvert.SerializeObject(model);
            subscriber.Publish(KeyRedisChannelNameForUpdate, serializeObject);
        }

        internal void PublishCleanAllCache(string prefix = "")
        {
            var model = new CacheUpdateModel
            {
                Key = "##removeall##" + prefix,
                Value = null,
                ValueType = null,
                Publisher = PublisherId,
                ExpireDate = DateTime.MinValue
            };

            var subscriber = Connection.GetSubscriber();
            var serializeObject = JsonConvert.SerializeObject(model);
            subscriber.Publish(KeyRedisChannelNameForUpdate, serializeObject);
        }

        #region Redis Connection

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection =
            new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(ConfigurationManager.GetParameter("RedisCacheServer") + ",syncTimeout=1000,connectTimeout=5000,abortConnect=false"));

        private static ConnectionMultiplexer Connection => LazyConnection.Value;

        private static IDatabase Db => Connection.GetDatabase();

        #endregion
    }
}