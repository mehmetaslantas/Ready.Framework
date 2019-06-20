using System;
using System.Globalization;

namespace Ready.Framework.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        ///     Decimal'i belirtilen formata ve kültürel bilgiye göre string'e çevirir.
        /// </summary>
        public static string ToPriceString(this decimal source, string currency = "")
        {
            return source.ToPriceString(CultureInfo.GetCultureInfo("tr-TR"), currency);
        }

        /// <summary>
        ///     Decimal'i belirtilen kültürel bilgiye göre string'e çevirir.
        /// </summary>
        public static string ToPriceString(this decimal source, CultureInfo culture, string currency = "")
        {
            if (string.IsNullOrEmpty(currency))
                return source.ToString("F", culture);
            return string.Format("{0} {1}", source.ToString("F", culture), currency.Trim());
        }

        /// <summary>
        ///     Ondalıklı değeri stringe çevirir.
        /// </summary>
        public static string ToStringInvariant(this decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Ondalıklı sayının tam sayı kısmını alır.
        /// </summary>
        public static string ToStringIntegral(this decimal value)
        {
            var left = Math.Floor(value);
            return string.Format("{0:0}", left);
        }

        /// <summary>
        ///     Ondalıklı sayının küsürat kısmını alır.
        /// </summary>
        public static string ToStringFraction(this decimal value)
        {
            var left = Math.Floor(value);
            var right = value - left;
            return string.Format("{0:.##}", right);
        }

        public static string ToRoundedPriceString(this decimal value, int partOfString = -1)
        {
            var roundedValue = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            var returnValue = string.Format("{0:.00}", roundedValue, CultureInfo.GetCultureInfo("tr-TR"));
            if (partOfString > -1 && partOfString < 2)
                returnValue = returnValue.Split(',', '.')[partOfString];
            if (string.IsNullOrWhiteSpace(returnValue)) returnValue = "0";
            return returnValue;
        }

        /// <summary>
        ///     Yüzdelik oranını hesaplar.
        /// </summary>
        public static decimal Percent(this decimal baseValue, decimal value)
        {
            if (value == 0)
                return 0;

            return Math.Floor((baseValue - value) / value * 100);
        }

        /// <summary>
        ///     Ondalıklı sayıyı string veri tipine çevirir.
        /// </summary>
        public static string ToPointString(this decimal? point)
        {
            if (point.HasValue)
                return point.Value.ToPointString();
            return string.Empty;
        }

        public static string ToPointString(this decimal point)
        {
            var numberFormat = new NumberFormatInfo();
            numberFormat.NumberDecimalSeparator = ".";
            return Convert.ToString(point, numberFormat);
        }

        public static string ToFormattedString(this decimal? value)
        {
            if (value.HasValue)
                return value.Value.ToFormattedString();
            return string.Empty;
        }

        public static string ToFormattedString(this decimal value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }
    }
}