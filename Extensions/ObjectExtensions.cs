using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;

namespace Ready.Framework.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNumeric(this object value)
        {
            var numericalTypes = new[] { typeof(int), typeof(double), typeof(float), typeof(long) };

            if (numericalTypes.FirstOrDefault(value.GetType()) != null)
                return true;

            var result = float.MinValue;
            return float.TryParse(value.ToString(), NumberStyles.Any, null, out result);
        }

        public static dynamic Merge(this object source, object destination)
        {
            if (source == null || destination == null)
                return source ?? destination ?? new ExpandoObject();

            dynamic expando = new ExpandoObject();
            var mergedItem = expando as IDictionary<string, object>;

            foreach (var prop in source.GetType().GetProperties())
                mergedItem[prop.Name] = prop.GetValue(source, null);

            foreach (var prop in destination.GetType().GetProperties())
                mergedItem[prop.Name] = prop.GetValue(destination, null);
            return mergedItem;
        }

        public static T ConvertTo<T>(this object source, T defaultValue = default(T)) //where T : struct, IConvertible
        {
            try
            {
                var type = typeof(T);

                if (source == null || source == DBNull.Value || source is string && string.IsNullOrEmpty(source as string))
                    return defaultValue;
                type = type.GetUnderlyingType();

                if (type.IsEnum)
                    return source.ToString().ToEnum<T>();
                if (type == typeof(Guid))
                {
                    if (source is string)
                        source = new Guid(source as string);
                    if (source is byte[])
                        source = new Guid(source as byte[]);
                }
                return (T)Convert.ChangeType(source, type, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw;
            }
        }

        public static bool TryCast<T>(this object source, out T result)
        {
            try
            {
                result = source.ConvertTo<T>();
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }
}