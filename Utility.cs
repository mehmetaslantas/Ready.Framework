using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Ready.Framework
{
    public static class Utility
    {
        public static string GenerateRandomPassword(int maxSize)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!?=.*".ToCharArray();
            var data = new byte[maxSize];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
            }
            var password = new StringBuilder(maxSize);
            foreach (byte b in data)
                password.Append(chars[b % (chars.Length)]);
            return password.ToString();
        }
        public static string GetQueryStringValue(string url, string queryString)
        {
            return System.Web.HttpUtility.ParseQueryString(url).Get(queryString);
        }
        public static string GetQueryStringValue(string url, string key, string seperator)
        {
            if (url.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) > -1 && url.Contains(seperator))
            {
                url = (new Regex(seperator)).Replace(url, "?", 1);
                return GetQueryStringValue(url, key);
            }
            return string.Empty;
        }
        public static string AddQueryStringValue(string url, string key, string value)
        {
            if (url.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) > -1)
                return url;

            var seperator = "?";
            if (url.IndexOf(seperator) > -1)
                seperator = "&";
            return string.Format("{0}{1}{2}={3}", url, seperator, key, value);
        }
        public static string[] GetMonthNames(CultureInfo culture = null)
        {
            if (culture == null)
                return DateTimeFormatInfo.CurrentInfo.MonthNames;
            else
                return DateTimeFormatInfo.GetInstance(culture).MonthNames;
        }
        public static object DeepClone(object Original)
        {
            byte[] bytes = null;
            object clonedObject = null;
            var formatter = new BinaryFormatter();
            try
            {
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, Original);
                    bytes = stream.ToArray();
                }
                using (var stream = new MemoryStream())
                {
                    clonedObject = formatter.Deserialize(stream);
                }
            }
            catch { }
            return clonedObject;
        }
        public static bool IsValidIdentityNumber(string sIdentityNumber)
        {
            long ATCNO, BTCNO;
            long C1, C2, C3, C4, C5, C6, C7, C8, C9, Q1, Q2;

            long nIdentityNumber = 0;
            if (string.IsNullOrWhiteSpace(sIdentityNumber) || sIdentityNumber.StartsWith("0") || sIdentityNumber.Length != 11 || !long.TryParse(sIdentityNumber, out nIdentityNumber) || nIdentityNumber < 0)
                return false;

            ATCNO = nIdentityNumber / 100;
            BTCNO = nIdentityNumber / 100;

            C1 = ATCNO % 10; ATCNO = ATCNO / 10;
            C2 = ATCNO % 10; ATCNO = ATCNO / 10;
            C3 = ATCNO % 10; ATCNO = ATCNO / 10;
            C4 = ATCNO % 10; ATCNO = ATCNO / 10;
            C5 = ATCNO % 10; ATCNO = ATCNO / 10;
            C6 = ATCNO % 10; ATCNO = ATCNO / 10;
            C7 = ATCNO % 10; ATCNO = ATCNO / 10;
            C8 = ATCNO % 10; ATCNO = ATCNO / 10;
            C9 = ATCNO % 10; ATCNO = ATCNO / 10;
            Q1 = ((10 - ((((C1 + C3 + C5 + C7 + C9) * 3) + (C2 + C4 + C6 + C8)) % 10)) % 10);
            Q2 = ((10 - (((((C2 + C4 + C6 + C8) + Q1) * 3) + (C1 + C3 + C5 + C7 + C9)) % 10)) % 10);

            return ((BTCNO * 100) + (Q1 * 10) + Q2 == nIdentityNumber);
        }
        /// <summary>
        ///Luhn Algoritması (Kredi kartı doğrulama)
        /// </summary>
        public static bool CheckLuhn(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return false;

            int sum = cardNumber.Where((e) => e >= '0' && e <= '9')
                            .Reverse()
                            .Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2))
                            .Sum((e) => e / 10 + e % 10);

            return sum % 10 == 0;
        }
        public static bool IsValidCardNumber(string cardNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(cardNumber))
                {
                    if (new Regex("^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13})$").IsMatch(cardNumber))
                        return CheckLuhn(cardNumber);
                }
            }
            catch { }
            return false;
        }
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(phoneNumber))
                    return new Regex("^[1-9][0-9]{9}$").IsMatch(phoneNumber);
            }
            catch { }
            return false;
        }
        public static string ClearQuotes(string str)
        {
            return str.Replace("\"", "&#34;").Replace("'", "&#39;");
        }
        public static string EscapeSpecialChars(string str, Dictionary<string, string> replaceOptions)
        {
            foreach (KeyValuePair<string, string> item in replaceOptions)
                str = str.Replace(item.Key, item.Value);
            return str;
        }
        public static DataTable CsvToDataTable(string filePath, string[] columns = null)
        {
            var dtResult = new DataTable();

            using (var streamReader = new StreamReader(filePath))
            {
                if (columns == null || columns.Length <= 0)
                {
                    string columnsLine = streamReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(columnsLine))
                        columns = columnsLine.Split(',');
                }
                if (columns != null && columns.Length > 0)
                {
                    foreach (var c in columns)
                        dtResult.Columns.Add(c, typeof(string));
                }
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] values = line.Split(',');
                        if (values != null && values.Length >= 1)
                        {
                            if (dtResult.Columns.Count < values.Length)
                            {
                                for (int i = 0; i < values.Length - dtResult.Columns.Count; i++)
                                    dtResult.Columns.Add("Col-" + (i + 1), typeof(string));
                            }
                            DataRow row = dtResult.NewRow();
                            for (int i = 0; i < dtResult.Columns.Count; i++)
                                row[i] = values[i];
                            dtResult.Rows.Add(row);
                        }
                    }
                }
            }
            return dtResult;
        }
        public static string ClearWordTags(string strIn)
        {
            try
            {
                string[] patterns = new string[] { @"!--(\w|\W)+?--", @"title(\w|\W)+?/title", @"\s?class=\w+", @"\s+style='\[^']+'", @"(meta|link|/?o:|/?style|/?div|/?st\d|/?head|/?html|body|/?body|/?span|!\[)\[^]*?", @"(\[^]+)+nbsp;(/\w+)+", @"\s+v:\w+=""\[^""]+""", @"(\n\r){2,}" };

                foreach (string replacePattern in patterns)
                    strIn = Regex.Replace(strIn, replacePattern, "", RegexOptions.IgnoreCase);
                strIn = strIn.Replace("<p class=\"MsoNormal\">&nbsp;</p>", "<p class=\"MsoNormal\">\\n</p>");
                return strIn.Replace("<>", "").Replace("\n", " ").Replace("\r", " ");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string ClearProtocols(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            return Regex.Replace(url, @"^(?:http(?:s)?:)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase);
        }
        public static string SwitchProtocol(string url, string protocol = "http://")
        {
            if (string.IsNullOrEmpty(url)) return url;
            if (string.IsNullOrEmpty(protocol))
                protocol = "http://";
            if (string.IsNullOrEmpty(url) || url.StartsWith(protocol, StringComparison.InvariantCultureIgnoreCase)) return url;
            return Regex.Replace(url, @"^(?:http(?:s)?:)?(?:\/\/)?", protocol, RegexOptions.IgnoreCase);
        }
        public static string EncodeUrl(string strIn)
        {
            if (string.IsNullOrEmpty(strIn)) return strIn;
            StringBuilder sbOut = new StringBuilder(strIn.Trim());
            sbOut = sbOut.Replace("-", "");
            sbOut = sbOut.Replace(" ", "-");
            sbOut = sbOut.Replace("  ", "-");
            sbOut = sbOut.Replace("ğ", "g");
            sbOut = sbOut.Replace("Ğ", "G");
            sbOut = sbOut.Replace("ü", "u");
            sbOut = sbOut.Replace("Ü", "U");
            sbOut = sbOut.Replace("ş", "s");
            sbOut = sbOut.Replace("Ş", "S");
            sbOut = sbOut.Replace("ç", "c");
            sbOut = sbOut.Replace("Ç", "C");
            sbOut = sbOut.Replace("ö", "o");
            sbOut = sbOut.Replace("Ö", "O");
            sbOut = sbOut.Replace("ı", "i");
            sbOut = sbOut.Replace("İ", "I");
            sbOut = sbOut.Replace("$", "-24");
            sbOut = sbOut.Replace("&", "-26");
            sbOut = sbOut.Replace("+", "-2B");
            sbOut = sbOut.Replace(",", "-2C");
            sbOut = sbOut.Replace("/", "-2F");
            sbOut = sbOut.Replace(":", "-3A");
            sbOut = sbOut.Replace(";", "-3B");
            sbOut = sbOut.Replace("=", "-3D");
            sbOut = sbOut.Replace("?", "-3F");
            sbOut = sbOut.Replace("@", "-40");
            sbOut = sbOut.Replace("'", "-22");
            sbOut = sbOut.Replace("<", "-3C");
            sbOut = sbOut.Replace(">", "-3E");
            sbOut = sbOut.Replace("#", "-23");
            sbOut = sbOut.Replace("%", "-25");
            sbOut = sbOut.Replace("{", "-7B");
            sbOut = sbOut.Replace("}", "-7D");
            sbOut = sbOut.Replace("|", "-7C");
            sbOut = sbOut.Replace("\\", "-5C");
            sbOut = sbOut.Replace("^", "-5E");
            sbOut = sbOut.Replace("~", "-7E");
            sbOut = sbOut.Replace("[", "-5B");
            sbOut = sbOut.Replace("]", "-5D");
            sbOut = sbOut.Replace("`", "-60");
            sbOut = sbOut.Replace(".", "-2E");
            sbOut = sbOut.Replace("\"", "-22");
            sbOut = sbOut.Replace("‘", "-91");
            sbOut = sbOut.Replace("’", "-92");
            sbOut = sbOut.Replace("ˆ", "-88");
            sbOut = sbOut.Replace("‚", "-83");
            return sbOut.ToString();
        }
    }
}
