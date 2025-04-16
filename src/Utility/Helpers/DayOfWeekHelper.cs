using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocuda.Utility.Helpers
{
    public class DayOfWeekHelper
    {
        private StringBuilder HandleRange(DayOfWeek? startRange, DayOfWeek? endRange)
        {
            if (startRange == null)
            {
                return null;
            }

            var sb = new StringBuilder(startRange.ToString());

            if (endRange != null)
            {
                // if the end range is one day it's not a range
                var seperator = endRange - startRange == 1
                    ? ", "
                    : "-";

                sb.Append(seperator)
                    .Append(endRange)
                    .Append(", ");
            }
            else
            {
                sb.Append(", ");
            }

            return sb;
        }

        public string ListDays()
        {
            return ListDays(null);
        }

        public string ListDays(IList<DayOfWeek> excludeDays)
        {
            var allDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
            var sb = new StringBuilder();

            if (excludeDays == null || excludeDays.Count == 0)
            {
                sb.Append(string.Join(", ", allDays));
            }
            else
            {
                DayOfWeek? startRange = null;
                DayOfWeek? endRange = null;

                var lastDay = allDays.Last();

                foreach (var day in allDays)
                {
                    if (excludeDays.Contains(day))
                    {
                        // if this day is blocked then yesterday was the end of a range
                        sb.Append(HandleRange(startRange, endRange));
                        startRange = null;
                        endRange = null;
                    }
                    else if (day == lastDay)
                    {
                        // if we're on the last day and not blocked we need to output the last date
                        if (startRange == null)
                        {
                            startRange = day;
                        }
                        else
                        {
                            endRange = day;
                        }
                        sb.Append(HandleRange(startRange, endRange));
                    }
                    else
                    {
                        // no block so we're in a range
                        if (startRange == null)
                        {
                            startRange = day;
                        }
                        else
                        {
                            endRange = day;
                        }
                    }
                }
            }

            // clear out ", " at the end if it's present
            var available = sb.ToString().Trim().Trim(',');
            if (available.Count(_ => _ == ',') > 1)
            {
                // if there's more than one comma, turn the last one into ", and"
                var lastComma = available.LastIndexOf(',');
                available = available.Remove(lastComma, 1)
                    .Insert(lastComma, ", and ");
            }

            return available;
        }
    }
}