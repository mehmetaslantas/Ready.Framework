using System;

namespace Ready.Framework.Extensions
{
    public static class LongExtensions
    {
        public static DateTime FromEpochTime(this long epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }
    }
}