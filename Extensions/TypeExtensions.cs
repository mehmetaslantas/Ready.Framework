using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ready.Framework.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsGenericAssignableFrom(this Type toType, Type fromType, out Type[] genericArguments)
        {
            if (!toType.IsGenericTypeDefinition ||
                fromType.IsGenericTypeDefinition)
            {
                genericArguments = null;
                return false;
            }

            if (toType.IsInterface)
                foreach (var interfaceCandidate in fromType.GetInterfaces())
                    if (interfaceCandidate.IsGenericType && interfaceCandidate.GetGenericTypeDefinition() == toType)
                    {
                        genericArguments = interfaceCandidate.GetGenericArguments();
                        return true;
                    }
                    else
                        while (fromType != null)
                        {
                            if (fromType.IsGenericType && fromType.GetGenericTypeDefinition() == toType)
                            {
                                genericArguments = fromType.GetGenericArguments();
                                return true;
                            }
                            fromType = fromType.BaseType;
                        }
            genericArguments = null;
            return false;
        }

        public static Type GetUnderlyingType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type) ?? type;
            return type;
        }

        public static object GetDefault(this Type type)
        {
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;

            if (type.ContainsGenericParameters)
                throw new ArgumentException(
                    "{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                    "> contains generic parameters, so the default value cannot be retrieved");

            if (type.IsPrimitive || !type.IsNotPublic)
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        "{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe Activator.CreateInstance method could not " +
                        "create a default instance of the supplied value type <" + type +
                        "> (Inner Exception message: \"" + e.Message + "\")", e);
                }

            throw new ArgumentException("{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                                        "> is not a publicly-visible type, so the default value cannot be retrieved");
        }

        public static T GetDefault<T>()
        {
            var type = typeof(T).GetUnderlyingType();
            return type.IsValueType ? (T)Activator.CreateInstance(type) : default(T);
        }

        public static bool HasDefaultValue<T>(this T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }

        public static Type GetRealType(this Type type)
        {
            return Assembly.GetAssembly(type).GetExportedTypes()
                .Where(type.IsAssignableFrom)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsInterface)
                .First();
        }

        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type, Assembly extensionsAssembly)
        {
            var methods = from t in extensionsAssembly.GetTypes()
                          where !t.IsGenericType && !t.IsNested
                          from m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                          where m.IsDefined(typeof(ExtensionAttribute), false)
                          where m.GetParameters()[0].ParameterType == type
                          select m;

            return methods;
        }

        public static MethodInfo GetExtensionMethod(this Type type, Assembly extensionsAssembly, string name)
        {
            return type.GetExtensionMethods(extensionsAssembly).FirstOrDefault(m => m.Name == name);
        }

        public static MethodInfo GetExtensionMethod(this Type type, List<Assembly> assemblies, string name)
        {
            MethodInfo method = null;
            var i = 0;
            do
            {
                method = type.GetExtensionMethods(assemblies[i]).FirstOrDefault(m => m.Name == name);
            } while (method == null && i < assemblies.Count);
            return method;
        }

        public static MethodInfo GetExtensionMethod(this Type type, Assembly extensionsAssembly, string name, Type[] types)
        {
            var methods = (from m in type.GetExtensionMethods(extensionsAssembly)
                           where m.Name == name
                                 && m.GetParameters().Count() == types.Length + 1
                           select m).ToList();

            if (!methods.Any())
                return default(MethodInfo);

            if (methods.Count() == 1)
                return methods.FirstOrDefault();

            foreach (var methodInfo in methods)
            {
                var parameters = methodInfo.GetParameters();

                var found = true;
                for (byte b = 0; b < types.Length; b++)
                {
                    found = true;
                    if (parameters[b].GetType() != types[b])
                        found = false;
                }

                if (found)
                    return methodInfo;
            }
            return default(MethodInfo);
        }
    }
}