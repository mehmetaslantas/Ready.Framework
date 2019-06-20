using System;

namespace Ready.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachingAttribute : Attribute
    {
        public enum CachingProfiles
        {
            Custom = -1,
            Short = 5,
            Smooth = 60,
            Medium = 1440,
            Aggressive = 43200
        }

        protected CachingProfiles CachingProfile { get; set; }

        public int CacheDuration { get; set; }

        public string DropCacheKey { get; set; }

        public CachingAttribute(CachingProfiles cachingProfile, int cacheDuration = 0)
        {
            CachingProfile = cachingProfile;
            CacheDuration = CachingProfile == CachingProfiles.Custom ? cacheDuration : (int)cachingProfile;
        }

        public CachingAttribute(CachingProfiles cachingProfile, string dropCacheKey, int cacheDuration = 0)
        {
            CachingProfile = cachingProfile;
            DropCacheKey = dropCacheKey;
            CacheDuration = CachingProfile == CachingProfiles.Custom ? cacheDuration : (int)cachingProfile;
        }
    }
}