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
        private const string ndash = "\u2013";

        private readonly ILocationHoursRepository _locationHoursRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationGroupRepository _locationGroupRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ILocationFeatureRepository _locationFeatureRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly ILocationHoursOverrideRepository _locationHoursOverrideRepository;

        public LocationService(ILogger<LocationService> logger,
            IDateTimeProvider dateTimeProvider,
            ILocationRepository locationRepository,
            ILocationGroupRepository locationGroupRepository,
            IFeatureRepository featureRepository,
            IGroupRepository groupRepository,
            ILocationFeatureRepository locationFeatureRepository,
            ILocationHoursRepository locationHoursRepository,
            ILocationHoursOverrideRepository locationHoursOverrideRepository)
            : base(logger, dateTimeProvider)
        {
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
            _locationGroupRepository = locationGroupRepository
                ?? throw new ArgumentNullException(nameof(locationGroupRepository));
            _featureRepository = featureRepository
                ?? throw new ArgumentNullException(nameof(featureRepository));
            _groupRepository = groupRepository
                ?? throw new ArgumentNullException(nameof(groupRepository));
            _locationFeatureRepository = locationFeatureRepository
                ?? throw new ArgumentNullException(nameof(locationFeatureRepository));
            _locationHoursRepository = locationHoursRepository
                ?? throw new ArgumentNullException(nameof(locationHoursRepository));
            _locationHoursOverrideRepository = locationHoursOverrideRepository
                ?? throw new ArgumentNullException(nameof(locationHoursOverrideRepository));
        }

        public async Task<Location> GetLocationByStubAsync(string stub)
        {
            return await _locationRepository.GetLocationByStub(stub);
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _locationRepository.GetAllLocations();
        }

        public async Task<ICollection<LocationHoursResult>> GetWeeklyHoursAsync(int locationId)
        {
            var results = new List<LocationHoursResult>();

            var location = await _locationRepository.FindAsync(locationId);

            if (location.IsAlwaysOpen)
            {
                for (int day = 0; day < DaysInWeek; day++)
                {
                    results.Add(new LocationHoursResult
                    {
                        Open = true,
                        DayOfWeek = (DayOfWeek)day,
                        IsCurrentlyOpen = true
                    });
                }
            }
            else
            {
                // Add override days
                var now = DateTime.Now;
                var firstDayOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var lastDayOfWeek = firstDayOfWeek.AddDays(DaysInWeek - 1);

                var overrides = await _locationHoursOverrideRepository.GetBetweenDatesAsync(
                    locationId, firstDayOfWeek, lastDayOfWeek);

                foreach (var dayOverride in overrides)
                {
                    results.Add(new LocationHoursResult
                    {
                        OpenTime = dayOverride.OpenTime,
                        CloseTime = dayOverride.CloseTime,
                        Open = dayOverride.Open,
                        DayOfWeek = dayOverride.Date.DayOfWeek,
                        IsOverride = true
                    });
                }

                // Fill in non-override days
                if (results.Count < DaysInWeek)
                {
                    var weeklyHours = await _locationHoursRepository.GetWeeklyHoursAsync(locationId);

                    var remainingDays = weeklyHours
                        .Where(_ => !results.Select(r => r.DayOfWeek).Contains(_.DayOfWeek));

                    foreach (var day in remainingDays)
                    {
                        results.Add(new LocationHoursResult
                        {
                            OpenTime = day.OpenTime,
                            CloseTime = day.CloseTime,
                            Open = day.Open,
                            DayOfWeek = day.DayOfWeek
                        });
                    }
                }

                // Set currently open
                foreach (var dayResult in results)
                {
                    if (dayResult.Open && dayResult.OpenTime <= now && dayResult.CloseTime >= now)
                    {
                        dayResult.IsCurrentlyOpen = true;
                    }
                }

                results = results.OrderBy(_ => _.DayOfWeek).ToList();
            }

            return results;
        }

        public async Task<LocationFeature> GetLocationFeatureByIds(int locationId, int featureId)
        {
            return await _locationFeatureRepository.GetLocationFeaturesByIds(locationId, featureId);
        }

        public async Task<List<Feature>> GetLocationsFeaturesAsync(string locationStub)
        {
            var location = await GetLocationByStubAsync(locationStub);
            var locationFeatures = await _locationFeatureRepository.GetLocationFeaturesByLocationId(location.Id);
            var features = new List<Feature>();
            foreach (var feature in locationFeatures)
            {
                features.Add(await _featureRepository.FindAsync(feature.FeatureId));
            }
            return features;
        }

        public async Task<List<Location>> GetLocationsNeighborsAsync(string locationStub)
        {
            var locationGroups = await _locationGroupRepository.GetGroupByLocationIdAsync((await GetLocationByStubAsync(locationStub)).Id);
            var locations = new List<Location>();
            foreach (var locationGroup in locationGroups)
            {
                if ((await _groupRepository.FindAsync(locationGroup.GroupId)).IsLocationRegion)
                {
                    var locationIds = await _locationGroupRepository.GetLocationsByGroupIdAsync(locationGroup.GroupId);
                    foreach (var location in locationIds)
                    {
                        if (location.HasSubscription)
                        {
                            locations.Add(await _locationRepository.FindAsync(location.LocationId));
                        }
                    }
                }
            }
            return locations;
        }
        public async Task<Group> GetLocationsNeighborGroup(string locationStub)
        {
            var locationGroups = await _locationGroupRepository.GetGroupByLocationIdAsync((await GetLocationByStubAsync(locationStub)).Id);
            foreach (var locationGroup in locationGroups)
            {
                var group = await _groupRepository.FindAsync(locationGroup.GroupId);
                if (group.IsLocationRegion)
                {
                    return group;
                }
            }
            return null;
        }

        public async Task<List<string>> GetFormattedWeeklyHoursAsync(int locationId, bool isStructuredData = false)
        {
            var location = await _locationRepository.FindAsync(locationId);
            if (location.IsAlwaysOpen)
            {
                return null;
            }

            var weeklyHours = await _locationHoursRepository.GetWeeklyHoursAsync(locationId);
            // Order weeklyHours to start on Monday
            weeklyHours = weeklyHours.OrderBy(_ => ((int)_.DayOfWeek + 1) % 8).ToList();

            var dayGroupings = new List<(List<DayOfWeek> DaysOfWeek, DateTime OpenTime, DateTime CloseTime)>();
            var closedDays = new List<DayOfWeek>();
            foreach (var day in weeklyHours)
            {
                if (day.Open)
                {
                    var (DaysOfWeek, OpenTime, CloseTime) = dayGroupings.LastOrDefault();
                    if (dayGroupings.Count > 0 && OpenTime == day.OpenTime
                        && CloseTime == day.CloseTime)
                    {
                        DaysOfWeek.Add(day.DayOfWeek);
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
            foreach (var (DaysOfWeek, OpenTime, CloseTime) in dayGroupings)
            {
                var days = isStructuredData ? GetFormattedDayGroupings(DaysOfWeek, true)
                    : GetFormattedDayGroupings(DaysOfWeek);
                var openTime = isStructuredData ? new StringBuilder(OpenTime.ToString("%H"))
                    : new StringBuilder(OpenTime.ToString("%h"));
                if (OpenTime.Minute != 0 || isStructuredData)
                {
                    openTime.Append(OpenTime.ToString(":mm"));
                }
                openTime.Append(isStructuredData ? "" : OpenTime.ToString(" tt").ToLower());

                var closeTime = isStructuredData ? new StringBuilder(CloseTime.ToString("%H"))
                    : new StringBuilder(CloseTime.ToString("%h"));
                if (CloseTime.Minute != 0 || isStructuredData)
                {
                    closeTime.Append(CloseTime.ToString(":mm"));
                }
                closeTime.Append(isStructuredData ? "" : CloseTime.ToString(" tt").ToLower());

                formattedDayGroupings.Add($"{days} {openTime}{ndash}{closeTime}");
            }

            if (closedDays.Count > 0 && !isStructuredData)
            {
                var formattedClosedDays = GetFormattedDayGroupings(closedDays);
                formattedDayGroupings.Add($"{formattedClosedDays} Closed");
            }

            return formattedDayGroupings;
        }

        private string GetFormattedDayGroupings(List<DayOfWeek> days, bool isStructuredData=false)
        {
            var dayFormatter = new DateTimeFormatInfo();
            if (days.Count == 1)
            {
                return isStructuredData ? dayFormatter.GetAbbreviatedDayName(days[0]).Substring(0,2)
                    : dayFormatter.GetAbbreviatedDayName(days[0]);
            }
            else
            {
                var firstDay = days[0];
                var lastDay = days.Last();

                if (days.Count == 2)
                {
                    if (isStructuredData)
                    {
                        return $"{dayFormatter.GetAbbreviatedDayName(firstDay).Substring(0, 2)}" +
                            $"{ndash}{dayFormatter.GetAbbreviatedDayName(lastDay).Substring(0, 2)}";
                    }
                    else
                    {
                        return $"{dayFormatter.GetAbbreviatedDayName(firstDay)}" +
                            $" & {dayFormatter.GetAbbreviatedDayName(lastDay)}";
                    }
                }
                else if (days.Count == lastDay - firstDay + 1)
                {
                    if (isStructuredData)
                    {
                        return $"{dayFormatter.GetAbbreviatedDayName(firstDay).Substring(0,2)}" +
                            $"{ndash}{dayFormatter.GetAbbreviatedDayName(lastDay).Substring(0, 2)}";
                    }
                    else
                    {
                        return $"{dayFormatter.GetAbbreviatedDayName(firstDay)}" +
                            $"{ndash}{dayFormatter.GetAbbreviatedDayName(lastDay)}";
                    }
                }
                else
                {
                    return string.Join(", ", days.Select(_ => dayFormatter.GetAbbreviatedDayName(_)));
                }
            }
        }

        public async Task<LocationHoursResult> GetCurrentStatusAsync(int locationId)
        {
            var location = await _locationRepository.FindAsync(locationId);

            if (location.IsAlwaysOpen)
            {
                return new LocationHoursResult
                {
                    Open = true,
                    IsCurrentlyOpen = true,
                    StatusMessage = "Open"
                };
            }

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
