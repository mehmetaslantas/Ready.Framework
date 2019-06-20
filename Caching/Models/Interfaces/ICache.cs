using System;
using System.Collections.Generic;
using Ready.Framework.Caching.Models.Enum;

namespace Ready.Framework.Caching.Models.Interfaces
{
    public interface ICache : IDisposable
    {
        bool UsePrefix { get; }

        bool NeedsSerialization { get; }

        CachingFormat CachingFormat { get; set; }

        object Get(string key);

        T Get<T>(string key);

        bool TryGet<T>(string key, out T value) where T : class;

        void Set(string key, object value, int duration);

        void Set(string key, object value, DateTime absoluteExpiration);

        bool IsExists(string key);

        void Remove(string key);

        bool RemoveAll(string prefix = "");

        IEnumerable<string> GetKeys(string prefix = "");
    }
}