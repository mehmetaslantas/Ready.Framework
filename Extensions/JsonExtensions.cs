using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace Ready.Framework.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj, bool compress = true, bool ignoreNulls = true, bool ignoreDefaultValues = false, string dateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
            bool stringEnumConverter = false)
        {
            var formatting = Formatting.Indented;
            if (compress)
                formatting = Formatting.None;

            if (string.IsNullOrEmpty(dateFormatString))
                dateFormatString = CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern;

            var settings = new JsonSerializerSettings { DateFormatString = dateFormatString };
            if (stringEnumConverter)
                settings.Converters.Add(new StringEnumConverter());

            if (ignoreNulls)
                settings.NullValueHandling = NullValueHandling.Ignore;

            if (ignoreDefaultValues)
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            if (obj != null)
                return JsonConvert.SerializeObject(obj, formatting, settings);

            return string.Empty;
        }

        public static T FromJson<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}