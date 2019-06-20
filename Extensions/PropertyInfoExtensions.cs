using System;
using System.Reflection;

namespace Ready.Framework.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static bool CanAssignValue(this PropertyInfo property, object value)
        {
            return value == null ? property.IsNullable() : property.PropertyType.IsInstanceOfType(value);
        }

        public static bool IsNullable(this PropertyInfo property)
        {
            return property.PropertyType.IsNullable();
        }

        public static void SetValue<T>(this PropertyInfo property, T instance, object value)
        {
            //if (property.DeclaringType != typeof(T))
            //    throw new ArgumentException("property's declaring type must be equal to typeof(T).");

            var constructedMethod = typeof(ObjectExtensions)
                .GetMethod("TryCast")
                .MakeGenericMethod(property.PropertyType);

            object valueSet = null;
            var parameters = new[] { value, null };

            if (Convert.ToBoolean(constructedMethod.Invoke(null, parameters)))
                valueSet = parameters[1];

            if (!property.CanAssignValue(valueSet))
                return;
            property.SetValue(instance, valueSet, null);
        }
    }
}