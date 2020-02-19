using System;
using System.Globalization;

namespace DataPowerTools.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// To a file date time string  e.g. "2_19_2020 12_03_36 PM"
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToFileDateTimeString(this DateTime time)
        {
            var fileDate = time.ToString(CultureInfo.InvariantCulture).Replace("/", "_").Replace(":", "_");

            return fileDate;
        }
    }
}