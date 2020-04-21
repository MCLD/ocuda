using System;

namespace Ocuda.Utility.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime RoundUp(this DateTime dateTime, TimeSpan roundTo)
        {
            return new DateTime((dateTime.Ticks + roundTo.Ticks - 1)
                / roundTo.Ticks * roundTo.Ticks, dateTime.Kind);
        }
    }
}
