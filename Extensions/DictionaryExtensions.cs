﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Ready.Framework.Extensions
{
    public static class DictionaryExtensions
    {
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            foreach (var item in source)
                if (!target.ContainsKey(item.Key))
                    target.Add(item.Key, item.Value);
        }

        public static DateTime? GetDateFromString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                if (value is string)
                {
                    DateTime date;
                    if (DateTime.TryParseExact(value as string, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        return date;
                }
            return null;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType()
                .GetProperties(bindingAttr).ToDictionary
                (
                    propInfo => propInfo.Name,
                    propInfo => propInfo.GetValue(source, null)
                );
        }
    }
}