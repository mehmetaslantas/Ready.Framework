using System;

namespace Ready.Framework.Extensions
{
    public static class GuidExtensions
    {
        /// <summary>
        ///     Guid değerinin null veya boş olup olmadığına bakar.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Değer null ise veya boş ise 1 döndürür.</returns>
        public static bool IsNullOrEmpty(this Guid target)
        {
            return target == null || target.Equals(Guid.Empty);
        }
    }
}