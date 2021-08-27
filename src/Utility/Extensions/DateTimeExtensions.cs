using System;

namespace Ocuda.Utility.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ApproxTimeAgo(this DateTime dateTime)
        {
            if (dateTime == default) { return null; }
            var diff = DateTime.Now - dateTime;
            if (diff.TotalDays > 1)
            {
                var days = Math.Floor(diff.TotalDays);
                return $"{days:n0} {(days == 1 ? "day" : "days")} ago";
            }
            if (diff.TotalHours > 1)
            {
                var hours = Math.Floor(diff.TotalHours);
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }

            return "recently";
        }

        public static DateTime CombineWithTime(this DateTime dateTime, DateTime time)
        {
            return dateTime.Date.Add(time.TimeOfDay);
        }

        public static DateTime RoundUp(this DateTime dateTime, TimeSpan roundTo)
        {
            return new DateTime((dateTime.Ticks + roundTo.Ticks - 1)
                / roundTo.Ticks * roundTo.Ticks, dateTime.Kind);
        }
    }
}