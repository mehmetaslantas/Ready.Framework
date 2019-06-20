using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Ready.Framework.Cryptography;

namespace Ready.Framework.Extensions
{
    public static class StringExtensions
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Cyrillic");
        public static readonly string JavascriptEmailAddressRegex = "^[_a-z0-9-]+(\\.[_a-z0-9-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,4})$";
        public static readonly string GUIDRegex = "^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$";


        public static bool In(this string value, params string[] stringValues)
        {
            foreach (var comparedValue in stringValues)
                if (string.Compare(value, comparedValue) == 0)
                    return true;

            return false;
        }

        public static string Format(this string value, params object[] args)
        {
            return string.Format(value, args);
        }

        public static bool IsMatch(this string value, string pattern)
        {
            var regex = new Regex(pattern);
            return value.IsMatch(regex);
        }

        public static bool IsMatch(this string value, Regex regex)
        {
            return regex.IsMatch(value);
        }

        public static string ChangeInvalidSpaces(this string value)
        {
            return value.Replace((char)160, (char)32);
        }

        /// <summary>
        ///     64 digit tabanlı string veriyi 8 bitlik unsigned integer diziye dönüştürür.
        /// </summary>
        public static byte[] ToByteArray(this string text)
        {
            return Convert.FromBase64String(text);
        }

        /// <summary>
        ///     8 bitlik unsigned integer diziyi 64 digit tabanlı string'e dönüştürür.
        /// </summary>
        public static string ToText(this byte[] buffer)
        {
            return Convert.ToBase64String(buffer);
        }

        public static T? ToNullable<T>(this string source) where T : struct
        {
            var result = new T?();
            try
            {
                if (!string.IsNullOrEmpty(source) && source.Trim().Length > 0)
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)converter.ConvertFromInvariantString(source);
                }
            }
            catch
            {
            }
            return result;
        }

        //public static T ConvertTo<T>(this string source, T defaultValue = default(T)) where T : struct, IConvertible
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(source))
        //        {
        //            var type = typeof(T).GetUnderlyingType();
        //            if (type.IsEnum)
        //                return source.ToEnum<T>();
        //            else
        //                return (T)Convert.ChangeType(source, type, CultureInfo.InvariantCulture);
        //        }
        //    }
        //    catch { return defaultValue; }
        //    return defaultValue;
        //}
        public static string ToPriceString(this string source)
        {
            return source.ToPriceString("tr-TR");
        }

        public static string ToPriceString(this string source, string cultureInfo)
        {
            return source.ToString(CultureInfo.GetCultureInfo(cultureInfo));
        }

        /// <summary>
        ///     Ondalıklı sayı biçimindeki string ifadedyi bir üst sayıya yuvarlayıp, sayının tam sayı kısmını alır.
        /// </summary>
        public static string ToRoundedValue(this string value)
        {
            var left = Math.Round(value.ToDecimal());
            return string.Format("{0:0}", left);
        }

        public static bool ToBoolean(this string value, string trueCondition = "true", bool defaultvalue = false)
        {
            try
            {
                bool result;
                if (bool.TryParse(value, out result))
                    return result;
                if (!string.IsNullOrEmpty(trueCondition))
                    return value.Equals(trueCondition, StringComparison.InvariantCultureIgnoreCase) ? true : false;
            }
            catch
            {
            }
            return defaultvalue;
        }

        public static short ToInt16(this string value)
        {
            short result = 0;

            if (!string.IsNullOrEmpty(value))
                short.TryParse(value, out result);

            return result;
        }

        public static int ToInt32(this string value)
        {
            var result = 0;

            if (!string.IsNullOrEmpty(value))
                int.TryParse(value, out result);

            return result;
        }

        public static long ToInt64(this string value)
        {
            long result = 0;

            if (!string.IsNullOrEmpty(value))
                long.TryParse(value, out result);

            return result;
        }

        /// <summary>
        ///     Ondalıklı sayı biçimindeki string ifadeyi kontrollü biçimde decimal değerine çevirir.
        /// </summary>
        public static decimal ToDecimal(this string value)
        {
            var numberFormat = new NumberFormatInfo();
            numberFormat.NumberDecimalSeparator = ",";
            return Convert.ToDecimal(value.Replace(".", ","), numberFormat);
        }

        public static DateTime ToDateTime(this string value, DateTime defaultValue = default(DateTime))
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            DateTime datetimeValue;
            if (!DateTime.TryParse(value, out datetimeValue))
                return defaultValue;
            return datetimeValue;
        }

        public static T ToEnum<T>(this string value, T defaultValue = default(T))
        {
            try
            {
                var type = typeof(T).GetUnderlyingType();
                if (!string.IsNullOrEmpty(value))
                {
                    if (Enum.IsDefined(type, value))
                        return (T)Enum.Parse(type, value, true);
                    if (value.IsNumeric())
                    {
                        int numeric;
                        if (int.TryParse(value, out numeric))
                            if (Enum.IsDefined(type, numeric))
                                return (T)Enum.ToObject(type, numeric);
                    }
                }
            }
            catch
            {
            }
            return defaultValue;
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var pos = text.IndexOf(search);
            if (pos < 0)
                return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ToDashCase(this string input)
        {
            var pattern = "[A-Z]";
            var dash = "-";
            return Regex.Replace(input, pattern,
                m => (m.Index == 0 ? string.Empty : dash) + m.Value.ToLowerInvariant());
        }

        public static string ToUpperTr(this string input)
        {
            return input.ToUpper(new CultureInfo("tr-TR"));
        }

        public static string ToLowerTr(this string input)
        {
            return input.ToLower(new CultureInfo("tr-TR"));
        }

        public static string ToSlug(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var str = string.Join("", value.Normalize(NormalizationForm.FormD)
                .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

            str = value.RemoveAccent().ClearTurkishChars().ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 200 ? str.Length : 200).Trim();
            str = Regex.Replace(str, @"\s", "-");
            str = Regex.Replace(str, @"-+", "-");
            return str;
        }

        /// <summary>
        ///     String'i parçalar.
        /// </summary>
        public static string[] SplitString(this string value, string regexPattern, int maxLength)
        {
            var splitted = new string[3];

            if (string.IsNullOrEmpty(value))
                return splitted;

            value = value.Trim();

            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);

            Match matchResults = null;
            var paragraphs = new Regex(regexPattern, RegexOptions.Singleline);
            matchResults = paragraphs.Match(value);
            if (matchResults.Success)
            {
                splitted[0] = matchResults.Groups[1].Value;
                splitted[1] = matchResults.Groups[2].Value;
                splitted[2] = matchResults.Groups[3].Value;
            }

            return splitted;
        }

        public static string AddLeadingZeros(this long value, int totalLength)
        {
            return value.AddLeadingZeros(totalLength, string.Empty);
        }

        public static string AddLeadingZeros(this long value, int totalLength, string prefix)
        {
            totalLength = totalLength - prefix.Length;
            return prefix + value.ToString().PadLeft(totalLength, '0');
        }

        public static string AddLeadingZeros(this string value, int totalLength, string prefix)
        {
            totalLength = totalLength - prefix.Length;
            return prefix + value.PadLeft(totalLength, '0');
        }

        public static string AddLeadingZeros(this int value, int totalLength, string prefix)
        {
            return ((long)value).AddLeadingZeros(totalLength, prefix);
        }

        public static string AddLeadingZeros(this byte value, int totalLength, string prefix)
        {
            return ((long)value).AddLeadingZeros(totalLength, prefix);
        }

        /// <summary>
        ///     Clear metodunu çalıştırarak kaynak içindeki belirtilen karakterleri siler.
        /// </summary>
        public static string Clear(this string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            return source.Trim('_').Clear(' ', '(', ')', '_', '-');
        }

        /// <summary>
        ///     Belirtilen karakterleri kaldırır string'den.
        /// </summary>
        public static string Clear(this string source, params char[] removeChars)
        {
            var destination = source;
            if (!string.IsNullOrEmpty(destination))
            {
                var splittedData = source.Split(removeChars, StringSplitOptions.RemoveEmptyEntries);
                destination = string.Concat(splittedData);
            }

            return destination;
        }

        public static string RemoveAccent(this string value)
        {
            var bytes = Encoding.GetBytes(value);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string UrlDecode(this string value)
        {
            return HttpUtility.UrlDecode(value);
        }

        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        public static string Zip(this string str)
        {
            var bytes1 = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes1))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    var bytes = new byte[4096];
                    int cnt;
                    while ((cnt = msi.Read(bytes, 0, bytes.Length)) != 0)
                        gs.Write(bytes, 0, cnt);
                }
                return mso.ToArray().ToJson();
            }
        }

        public static string Unzip(this string str)
        {
            using (var msi = new MemoryStream(str.ToByteArray()))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    var bytes = new byte[4096];
                    int cnt;
                    while ((cnt = gs.Read(bytes, 0, bytes.Length)) != 0)
                        mso.Write(bytes, 0, cnt);
                }
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static string Encrypt(this string chipperText, string encryptionKey = "")
        {
            return CryptoManager.Encrypt(chipperText, encryptionKey);
        }

        public static string Decrypt(this string richText, string decryptionKey = "")
        {
            return CryptoManager.Decrypt(richText, decryptionKey);
        }

        public static string ComputeHash(this string plainText, string saltText = "", HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding encoding = null)
        {
            return CryptoManager.ComputeHashByAlgorithm(plainText, saltText, algorithm, encoding);
        }

        public static string GetMd5Hash(this string plainText, string saltText = "", Encoding encoding = null, bool useToBase64String = true)
        {
            return CryptoManager.GetMd5Hash(plainText, saltText, encoding, useToBase64String);
        }

        public static string GetSha256Hash(this string plainText, string saltText = "", Encoding encoding = null, bool useToBase64String = true)
        {
            return CryptoManager.GetSha256Hash(plainText, saltText, encoding, useToBase64String);
        }

        public static bool IsEmail(this string value)
        {
            return !string.IsNullOrEmpty(value) && value.IsMatch(JavascriptEmailAddressRegex);
        }

        /// <summary>
        ///     Özel türkçe harfleri latin harflere çevirir.
        /// </summary>
        public static string ClearTurkishChars(this string value)
        {
            var sb = new StringBuilder(value);
            sb = sb.Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c")
                .Replace("İ", "I")
                .Replace("Ğ", "G")
                .Replace("Ü", "U")
                .Replace("Ş", "S")
                .Replace("Ö", "O")
                .Replace("Ç", "C");

            return sb.ToString();
        }

        public static string Replace(this string text, string regexPattern)
        {
            if (string.IsNullOrEmpty(regexPattern))
                throw new ArgumentNullException(nameof(regexPattern));

            Regex regex = new Regex(regexPattern);
            return regex.Replace(text, string.Empty);
        }

        public static bool HasValue(this string value, string[] array)
        {
            foreach (var item in array)
                if (value.IndexOf(item, StringComparison.InvariantCultureIgnoreCase) > -1)
                    return true;
            return false;
        }

        public static bool EndsWith(this string value, string[] array)
        {
            foreach (var item in array)
                if (value.EndsWith(item, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        public static string ArrangeStringAgainstRiskyChar(this string value)
        {
            if (!string.IsNullOrEmpty(value))
                value = value.Replace("<", "").Replace(">", "").Replace("(", "").Replace(")", "").Replace(";", "").Replace("&", "").Replace("+", "").Replace("%", "").Replace("#", "").Replace("$", "")
                    .Replace("\\", "").Replace("*", "").Replace("|", "").Replace("'", "").Replace("script", "");
            return value;
        }

        public static string ClearWordTags(this string value)
        {
            return Utility.ClearWordTags(value);
        }

        public static string ClearProtocols(this string url)
        {
            return Utility.ClearProtocols(url);
        }

        public static string SwitchProtocol(this string url, string protocol = "http://")
        {
            return Utility.SwitchProtocol(url, protocol);
        }

        public static string ToSecureUrl(this string url)
        {
            return url.SwitchProtocol("https://");
        }

        public static string GetQueryStringValue(this string url, string queryString)
        {
            return Utility.GetQueryStringValue(url, queryString);
        }

        public static string GetQueryStringValue(this string url, string key, string seperator)
        {
            return Utility.GetQueryStringValue(url, key, seperator);
        }
    }
}