using System;
using System.Collections.Generic;

namespace Ready.Framework.Extensions
{
    public static class ArrayExtensions
    {
        public static void InsertItems<T>(this T[] array, IEnumerable<T> items)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (index >= array.Length)
                    break;

                array[index] = item;
                index++;
            }
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static List<T> ToList<T>(this Array items, Func<object, T> mapFunction)
        {
            if (items == null || mapFunction == null)
                return new List<T>();

            var list = new List<T>();
            for (var i = 0; i < items.Length; i++)
            {
                var val = mapFunction(items.GetValue(i));
                if (val != null)
                    list.Add(val);
            }
            return list;
        }
    }
}