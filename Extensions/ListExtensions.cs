using System;
using System.Collections.Generic;
using System.Linq;

namespace Ready.Framework.Extensions
{
    public static class ListExtensions
    {
        public static bool Contains(this List<string> source, string value, StringComparison? comparison = null)
        {
            return comparison.HasValue ? source.Any(s => s.Equals(value, comparison.Value)) : source.Contains(value);
        }

        /// <summary>
        ///     Removes all elements from the list that satisfy the condition.  Returns the list that was passed in (minus removed
        ///     elements) for chaining.
        /// </summary>
        public static IList<T> RemoveWhere<T>(this IList<T> source, Predicate<T> predicate)
        {
            if (source == null || source.Count == 0)
                return source;
            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (predicate(item))
                {
                    source.RemoveAt(i);
                    i--;
                }
            }
            return source;
        }

        public static IList<T> AddAll<T>(this IList<T> destination, IEnumerable<T> source)
        {
            foreach (var item in source)
                destination.Add(item);

            return destination;
        }
    }
}