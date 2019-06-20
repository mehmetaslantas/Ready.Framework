using System;

namespace Ready.Framework.Caching.Models.Base
{
    public class CacheUpdateModel
    {
        public string Key { get; set; }

        public object Value { get; set; }

        public Type ValueType { get; set; }

        public string Publisher { get; set; }

        public DateTime ExpireDate { get; set; }
    }
}