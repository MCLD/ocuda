using System;
using System.Collections.Generic;
using System.Linq;
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
                    if ((int)nextOpen.DayOfWeek == ((int)now.DayOfWeek + 1) % DaysInWeek) {
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
