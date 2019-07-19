using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class LocationService : BaseService<LocationService>
    {
        private const int DaysInWeek = 7;

        private readonly ILocationHoursRepository _locationHoursRepository;
        private readonly ILocationHoursOverrideRepository _locationHoursOverrideRepository;

        public LocationService(ILogger<LocationService> logger,
            IDateTimeProvider dateTimeProvider,
            ILocationHoursRepository locationHoursRepository,
            ILocationHoursOverrideRepository locationHoursOverrideRepository)
            : base(logger, dateTimeProvider)
        {
            _locationHoursRepository = locationHoursRepository
                ?? throw new ArgumentNullException(nameof(locationHoursRepository));
            _locationHoursOverrideRepository = locationHoursOverrideRepository
                ?? throw new ArgumentNullException(nameof(locationHoursOverrideRepository));
        }

        public async Task<ICollection<LocationHours>> GetWeeklyHoursAsync(int locationId)
        {
            return await _locationHoursRepository.GetWeeklyHoursAsync(locationId);
        }

        public async Task<List<string>> GetFormattedWeeklyHoursAsync(int locationId)
        {
            var weeklyHours = await _locationHoursRepository.GetWeeklyHoursAsync(locationId);
            // Order weeklyHours to start on Monday
            weeklyHours = weeklyHours.OrderBy(_ => ((int)_.DayOfWeek + 1) % 8).ToList();

            var dayGroupings = new List<(List<DayOfWeek> DaysOfWeek, DateTime OpenTime, DateTime CloseTime)>();
            var closedDays = new List<DayOfWeek>();
            foreach (var day in weeklyHours)
            {
                if (day.Open)
                {
                    var lastDayGrouping = dayGroupings.LastOrDefault();
                    if (dayGroupings.Count > 0 && lastDayGrouping.OpenTime == day.OpenTime 
                        && lastDayGrouping.CloseTime == day.CloseTime)
                    {
                        lastDayGrouping.DaysOfWeek.Add(day.DayOfWeek);
                    }
                    else
                    {
                        dayGroupings.Add((
                            DaysOfWeek: new List<DayOfWeek> { day.DayOfWeek },
                            OpenTime: day.OpenTime.Value,
                            CloseTime: day.CloseTime.Value));
                    }
                }
                else
                {
                    closedDays.Add(day.DayOfWeek);
                }
            }

            var formattedDayGroupings = new List<string>();
            foreach (var grouping in dayGroupings)
            {
                var days = GetFormattedDayGroupings(grouping.DaysOfWeek);

                var openTime = new StringBuilder(grouping.OpenTime.ToString("%h"));
                if (grouping.OpenTime.Minute != 0)
                {
                    openTime.Append(grouping.OpenTime.ToString(":mm"));
                }
                openTime.Append(grouping.OpenTime.ToString(" tt").ToLower());

                var closeTime = new StringBuilder(grouping.CloseTime.ToString("%h"));
                if (grouping.CloseTime.Minute != 0)
                {
                    closeTime.Append(grouping.CloseTime.ToString(":mm"));
                }
                closeTime.Append(grouping.CloseTime.ToString(" tt").ToLower());

                formattedDayGroupings.Add($"{days} {openTime.ToString()} \u2014 {closeTime.ToString()}");
            }

            if (closedDays.Count > 0)
            {
                var formattedClosedDays = GetFormattedDayGroupings(closedDays);
                formattedDayGroupings.Add($"{formattedClosedDays} Closed");
            }

            return formattedDayGroupings;
        }

        private string GetFormattedDayGroupings(List<DayOfWeek> days)
        {
            var dayFormatter = new DateTimeFormatInfo();
            if (days.Count == 1)
            {
                return dayFormatter.GetAbbreviatedDayName(days.First());
            }
            else
            {
                var firstDay = days.First();
                var lastDay = days.Last();

                if (days.Count == lastDay - firstDay + 1)
                {
                    return $"{dayFormatter.GetAbbreviatedDayName(firstDay)} \u2014 {dayFormatter.GetAbbreviatedDayName(lastDay)}";
                }
                else if (days.Count == 2)
                {
                    return $"{dayFormatter.GetAbbreviatedDayName(firstDay)} & {dayFormatter.GetAbbreviatedDayName(lastDay)}";
                }
                else
                {
                    return string.Join(", ", days.Select(_ => dayFormatter.GetAbbreviatedDayName(_)));
                }
            }
        }

        public async Task<LocationHoursResult> GetCurrentStatusAsync(int locationId)
        {
            var now = _dateTimeProvider.Now;

            var result = new LocationHoursResult();

            var todayOverride = await _locationHoursOverrideRepository
                .GetByDateAsync(locationId, now);

            if (todayOverride != null)
            {
                result.Open = todayOverride.Open;
                result.OpenTime = todayOverride.OpenTime;
                result.CloseTime = todayOverride.CloseTime;
            }
            else
            {
                var todayHours = await _locationHoursRepository.GetByDayOfWeek(locationId, now);

                result.Open = todayHours.Open;
                result.OpenTime = todayHours.OpenTime;
                result.CloseTime = todayHours.CloseTime;
            }

            var showNextOpen = false;

            if (result.Open)
            {
                if (!result.OpenTime.HasValue || !result.CloseTime.HasValue)
                {
                    result.StatusMessage = "Open";
                    result.IsCurrentlyOpen = true;
                }
                else if (result.OpenTime.Value.TimeOfDay > now.TimeOfDay)
                {
                    result.StatusMessage = $"Opens at {result.OpenTime.Value.ToString("h:mm tt")}";
                }
                else if (result.CloseTime.Value.TimeOfDay > now.TimeOfDay)
                {
                    result.StatusMessage = $"Open until {result.CloseTime.Value.ToString("h:mm tt")}";
                    result.IsCurrentlyOpen = true;
                }
                else
                {
                    result.StatusMessage = "Closed";
                    showNextOpen = true;
                }
            }
            else
            {
                result.StatusMessage = "Closed today";
                showNextOpen = true;
            }

            if (showNextOpen)
            {
                // get future overrides less than a week away for the location
                var futureOverrides = (await _locationHoursOverrideRepository
                    .GetBetweenDatesAsync(locationId, now.AddDays(1), now.AddDays(DaysInWeek - 1)))
                    .Select(_ => new LocationHours
                    {
                        Open = _.Open,
                        OpenTime = _.OpenTime,
                        DayOfWeek = _.Date.DayOfWeek
                    });

                // get branch hours for the week excluding the current day and days that have overrides
                var weeklyBranchHours = await _locationHoursRepository
                    .GetWeeklyHoursAsync(locationId);
                weeklyBranchHours = weeklyBranchHours
                    .Where(_ => _.DayOfWeek != now.DayOfWeek
                        && !futureOverrides.Select(d => d.DayOfWeek).Contains(_.DayOfWeek))
                    .ToList();

                // combine the lists and order by the next upcoming day to get the next branch opening
                var nextOpen = futureOverrides
                    .Concat(weeklyBranchHours)
                    .Where(_ => _.Open)
                    .OrderBy(_ => (_.DayOfWeek - now.DayOfWeek + DaysInWeek) % DaysInWeek)
                    .FirstOrDefault();

                if (nextOpen != null)
                {
                    var nextDay = "";
                    var blah = now.DayOfWeek + 10;
                    if ((int)nextOpen.DayOfWeek == ((int)now.DayOfWeek + 1) % DaysInWeek)
                    {
                        nextDay = "tomorrow";
                    }
                    else
                    {
                        nextDay = nextOpen.DayOfWeek.ToString();
                    }

                    result.StatusMessage = $"Opens {nextDay} at {nextOpen.OpenTime.Value.ToString("h:mm tt")}";
                }
            }

            return result;
        }
    }
}
