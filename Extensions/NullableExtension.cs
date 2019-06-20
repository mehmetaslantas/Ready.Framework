namespace Ready.Framework.Extensions
{
    public static class NullableExtension
    {
        public static T? AsNullable<T>(this T value) where T : struct
        {
            return value;
        }
    }
}