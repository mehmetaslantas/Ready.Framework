//using System;
//using System.Collections.Generic;
//using Ready.Framework.Caching.Models.Enum;
//using Ready.Framework.Caching.Models.Interfaces;

//namespace Ready.Framework.Caching.Models
//{
//    internal class Memcached : ICache
//    {
//        private static volatile Memcached _current;

//        private static volatile object _locker = new object();

//        private MemcachedClient Client { get; } = new MemcachedClient(new MemcachedClientConfiguration());

//        public static Memcached Current
//        {
//            get
//            {
//                if (_current != null)
//                    return _current;

//                lock (_locker)
//                {
//                    if (_current == null)
//                        _current = new Memcached();
//                }

//                return _current;
//            }
//        }

//        public bool UsePrefix { get; } = true;

//        public bool NeedsSerialization { get; } = true;

//        public CachingFormat CachingFormat { get; set; }

//        public object Get(string key)
//        {
//            return Client.Get(key);
//        }

//        public T Get<T>(string key)
//        {
//            return Client.Get<T>(key);
//        }

//        public bool TryGet<T>(string key, out T o) where T : class
//        {
//            try
//            {
//                if (!IsExists(key))
//                {
//                    o = default(T);
//                    return false;
//                }
//                o = Get<T>(key);
//            }
//            catch(Exception ex)
//            {
//                o = default(T);
//                return false;
//            }
//            return true;
//        }

//        public void Set(string key, object value, int duration)
//        {
//            Set(key, value, DateTime.Now.AddMinutes(duration));
//        }

//        public void Set(string key, object value, DateTime absoluteExpiration)
//        {
//            Client.Store(StoreMode.Set, key, value, absoluteExpiration);
//        }

//        public bool IsExists(string key)
//        {
//            return Client.Get(key) != null;
//        }

//        public void Remove(string key)
//        {
//            try
//            {
//                Client.Remove(key);
//            }
//            catch
//            {
//                // ignored
//            }
//        }

//        public bool RemoveAll(string prefix)
//        {
//            return RemoveAll();
//        }

//        public IEnumerable<string> GetKeys(string prefix = "")
//        {
//            return null;
//        }

//        public void Dispose()
//        {
//            Client.Dispose();
//        }

//        public void Set(string key, object o)
//        {
//            Client.Store(StoreMode.Set, key, o);
//        }

//        public bool RemoveAll()
//        {
//            return false;
//        }
//    }
//}