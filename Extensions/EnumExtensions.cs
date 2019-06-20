using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Ready.Framework.Extensions
{
    public static class EnumExtensions
    {
        public static bool IsDefined<T>(this T value) where T : struct, IConvertible
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public static int ToInt32(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        public static byte ToByte(this Enum value)
        {
            return Convert.ToByte(value);
        }

        public static string GetValue<T>(this Enum value)
        {
            return Convert.ChangeType(value, typeof(T)).ToString();
        }


        public static string GetEnumDescription(this Enum value)
        {
            if (value == null)
            {
                return "";
            }

            DescriptionAttribute attribute = value.GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault() as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }


        public static List<TEnum> ToList<TEnum>()
        {
            Type type = typeof(TEnum);

            if (type.BaseType == typeof(Enum))
            {
                throw new ArgumentException("T must be type of System.Enum");
            }

            Array values = Enum.GetValues(type);
            if (values.Length > 0)
            {
                return values.Cast<TEnum>().ToList();
            }

            return null;
        }
    }
}