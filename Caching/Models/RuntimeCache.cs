using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Ready.Framework.Caching.Models.Enum;
using Ready.Framework.Caching.Models.Interfaces;

namespace Ready.Framework.Caching.Models
{
    internal sealed class RuntimeCache : ICache
    {
        private static readonly object AccessLocker = new object();
        private static volatile RuntimeCache _current;
        private ObjectCache _context;

        internal RuntimeCache()
        {
            _context = MemoryCache.Default;
        }

        public static RuntimeCache Current
        {
            get
            {
                if (_current == null)
                    lock (AccessLocker)
                    {
                        if (_current == null)
                            _current = new RuntimeCache();
                    }
                return _current;
            }
        }

        public bool UsePrefix => true;

        public bool NeedsSerialization => false;

        public CachingFormat CachingFormat { get; set; }

        public IEnumerable<string> GetKeys(string prefix = "")
        {
            if (string.IsNullOrEmpty(prefix))
                return _context.Select(kvp => kvp.Key);

            if (UsePrefix && !prefix.StartsWith(CacheManager.CachingPrefix))
                prefix = CacheManager.CachingPrefix + prefix;

            return _context.Where(kvp => kvp.Key.StartsWith(prefix)).Select(kvp => kvp.Key);
        }

        public object Get(string key)
        {
            return _context[key];
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return default(T);
            return (T)_context.Get(key);
        }

        public bool TryGet<T>(string key, out T value) where T : class
        {
            try
            {
                if (!IsExists(key))
                {
                    value = default(T);
                    return false;
                }
                value = (T)_context[key];
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
            Set(key, value, DateTime.Now.AddMinutes(duration));
        }

        public void Set(string key, object value, DateTime absoluteExpiration)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (value == null)
                return;

            _context.Set(key, value, absoluteExpiration);
        }

        public bool IsExists(string key)
        {
            return _context[key] != null;
        }

        public void Remove(string key)
        {
            _context.Remove(key);
        }

        public bool RemoveAll(string prefix)
        {
            var cacheKeys = GetKeys(prefix);
            foreach (var cacheKey in cacheKeys)
                Remove(cacheKey);
            return true;
        }

        public void Dispose()
        {
            _context = null;
        }

        public IEnumerable<string> GetEnumarableKeys(string prefix)
        {
            foreach (var item in _context)
                if (item.Key.StartsWith(prefix))
                    yield return item.Key;
        }

        public bool RemoveAll()
        {
            foreach (var element in _context)
                Remove(element.Key);
            return true;
        }
    }
}