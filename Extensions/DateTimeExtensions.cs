using System;
using System.Globalization;
using System.Threading;

namespace Ready.Framework.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime UnixStart => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long UnixTimestampNow => DateTime.UtcNow.ToUnixTimestamp();

        /// <summary>
        ///     Tarih değerinin uygunluğunu kontrol eder ve uygun olmayan değerler için sql server'ın alt sınır tarih değerini
        ///     döner.
        /// </summary>
        public static DateTime ValidateDate(this DateTime? datetime)
        {
            var minValue = new DateTime(1753, 1, 1);
            return !datetime.HasValue || datetime.Value <= minValue ? minValue : datetime.Value;
        }

        /// <summary>
        ///     Verilen tarih değerini '23 Ekim 1988, Çarşamba' formatında string ifadeye çevirir.
        /// </summary>
        public static string ToTrString(this DateTime datetime, string regex = "dd MMM yyyy, dddd")
        {
            return datetime.ToString(regex, new CultureInfo("tr-TR"));
        }

        public static string AsFileName(this DateTime datetime, string baseFileName = "")
        {
            if (string.IsNullOrEmpty(baseFileName)) return datetime.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
            return string.Format("{0}-{1}", baseFileName.ToSlug(), datetime.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture));
        }

        public static string AsDirectoryName(this DateTime datetime, string regex = "yyyy-MM-dd")
        {
            return datetime.ToString(regex, CultureInfo.InvariantCulture);
        }

        public static DateTime FirstDateOfWeek(this DateTime datetime, int year, int weekOfYear)
        {
            var firstDay = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - firstDay.DayOfWeek;

            var firstThursday = firstDay.AddDays(daysOffset);
            var calendar = Thread.CurrentThread.CurrentCulture.Calendar;
            var firstWeek = calendar.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNumber = weekOfYear;
            if (firstWeek <= 1) weekNumber -= 1;

            datetime = firstThursday.AddDays(weekNumber * 7);
            return datetime.AddDays(-3);
        }

        public static DateTime Trim(this DateTime datetime, long roundTicks)
        {
            return new DateTime(datetime.Ticks - datetime.Ticks % roundTicks);
        }

        public static DateTime TrimTime(this DateTime datetime)
        {
            return datetime.Trim(TimeSpan.TicksPerDay);
        }

        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType)
        {
            return new TimeSpan(
                Convert.ToInt64(Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks,
                    roundingType
                )) * roundingInterval.Ticks
            );
        }

        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval)
        {
            return Round(time, roundingInterval, MidpointRounding.AwayFromZero);
        }

        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval)
        {
            return new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
        }

        public static DateTime RoundMinute(this DateTime datetime, int accumulator)
        {
            var result = datetime.Round(TimeSpan.FromMinutes(accumulator));
            if (result < datetime)
                result = result.AddMinutes(accumulator);
            return result;
        }

        public static double UnixTicks(this DateTime datetime)
        {
            var unixStart = UnixStart;
            var universal = datetime.ToUniversalTime();
            var timespan = new TimeSpan(universal.Ticks - unixStart.Ticks);
            return timespan.TotalMilliseconds;
        }

        public static long ToUnixTimestamp(this DateTime datetime)
        {
            return (long)datetime.ToUniversalTime().Subtract(UnixStart).TotalSeconds;
        }

        public static DateTime Add(this DateTime datetime, TimeSpan value)
        {
            try
            {
                if (value.Ticks > 0)
                {
                    if (DateTime.MaxValue - datetime >= value) return DateTime.MaxValue;
                    return datetime.Add(value);
                }
            }
            catch
            {
            }
            return datetime;
        }

        public static int DaysInMonth(this DateTime datetime)
        {
            return DateTime.DaysInMonth(datetime.Year, datetime.Month);
        }

        public static DateTime FixAbsoluteDay(this DateTime datetime)
        {
            return datetime.SetAbsoluteDay(datetime.Date.Day);
        }

        public static DateTime SetAbsoluteDay(this DateTime datetime, int absoluteDay)
        {
            if (absoluteDay <= 0 || datetime == DateTime.MaxValue || datetime == DateTime.MinValue) return datetime;
            if (absoluteDay <= 28)
                return new DateTime(datetime.Year, datetime.Month, absoluteDay, datetime.Date.Hour, datetime.Date.Minute, datetime.Date.Second, datetime.Date.Millisecond);
            var nextMonth = datetime.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, 1);
        }
    }
}