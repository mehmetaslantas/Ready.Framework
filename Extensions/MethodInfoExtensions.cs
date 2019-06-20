using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Ready.Framework.Attributes;
using Ready.Framework.Authorization;
using Ready.Framework.Caching;
using Ready.Framework.Exceptions;
using Ready.Framework.Service.Messaging;
using Ready.Framework.Service.Messaging.Enum;
using Ready.Framework.Service.Models;
#pragma warning disable 168

namespace Ready.Framework.Extensions
{
    internal static class MethodInfoExtensions
    {
        internal static bool HasServiceResponse(this MethodInfo methodInfo)
        {
            return methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ServiceResponse<>) ||
                   !methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType == typeof(ServiceResponse);
        }

        internal static bool HasGenericServiceResponse(this MethodInfo methodInfo)
        {
            return methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ServiceResponse<>);
        }

        internal static bool IsCaching(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<CachingAttribute>() != null && methodInfo.ReturnType != typeof(void);
        }

        internal static string GetCachePrefix<TClient>(this TClient methodInfo, string serviceVersion = null) where TClient : class, IClient
        {
            var cachePrefix = string.Join("-", typeof(TClient).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Select(propInfo =>
                    {
                        var cachePrefixAttribute = propInfo.GetCustomAttribute<CachePrefixAttribute>();
                        return cachePrefixAttribute != null ? new { cachePrefixAttribute.Order, Text = cachePrefixAttribute.Text ?? propInfo.Name } : null;
                    }
                ).Where(x => x != null)
                .OrderBy(x => x.Order)
                .Select(x => x.Text));

            return $"{cachePrefix}{serviceVersion}";
        }

        internal static void CheckDropCache(this MethodInfo methodInfo)
        {
            var dropCacheAttribute = methodInfo.GetCustomAttribute<DropCacheAttribute>();

            if (dropCacheAttribute?.DropCacheKeys == null)
                return;

            foreach (var prefix in dropCacheAttribute.DropCacheKeys)
                try
                {
                    CacheManager.RemoveAll(prefix);
                }
                catch (Exception e)
                {
                    // ignored
                }
        }

        internal static object ToMessageResponse<T>(this T ex, MethodInfo methodInfo) where T : Exception
        {
            var result = Activator.CreateInstance(methodInfo.ReturnType);

            var messageException = ex as MessageException ?? ex.InnerException as MessageException;

            var errorMessage = messageException?.Message ?? new Message
            {
                Code = "MWFRWK0023",
                Title = "Error",
                Text = "We are currently unable to perform your request.",
                HttpStatus = HttpStatusCode.ServiceUnavailable,
                IconType = MessageIconType.Error,
                Buttons = new List<MessageButton>
                {
                    new MessageButton
                    {
                        Text = "OK",
                        Type = MessageButtonType.Ok
                    }
                }
            };

            var propertyInfo = methodInfo.ReturnType.GetProperty(nameof(ServiceResponse.Message));
            propertyInfo?.SetValue(result, errorMessage);

            return result;
        }

        internal static string GenerateCacheKey(this MethodInfo methodInfo, object[] args, string cachePrefix = null)
        {
            var parametersKey = string.Join("###", methodInfo.GetParameters().Select((parameter, index) => parameter.ParameterType.IsSimpleType()
                ? $"{parameter.ParameterType.FullName}##{args[index]}"
                : $"{parameter.ParameterType.FullName}##{args[index]?.ToJson()}"));

            var methodKey = string.Join('$', methodInfo.DeclaringType?.FullName, methodInfo.Name, parametersKey);

            var result = $"{cachePrefix}-{methodKey}";

            var caching = methodInfo.GetCustomAttribute<CachingAttribute>();
            if (!string.IsNullOrEmpty(caching?.DropCacheKey))
                result = $"{caching.DropCacheKey}-{result}";

            return result;
        }

        private static bool IsSimpleType(this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}