using System;

namespace Ready.Framework.Attributes
{
    public class DropCacheAttribute : Attribute
    {
        public string[] DropCacheKeys { get; }

        public DropCacheAttribute(params string[] dropCacheKeys)
        {
            DropCacheKeys = dropCacheKeys;
        }
    }
}