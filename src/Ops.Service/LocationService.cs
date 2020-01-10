using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class LocationService : BaseService<LocationService>, ILocationService
    {
        private const string ndash = "\u2013";

        private readonly ILocationRepository _locationRepository;
        private readonly ILocationHoursRepository _locationHoursRepository;

        public LocationService(ILogger<LocationService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILocationRepository locationRepository,
            ILocationHoursRepository locationHoursRepository)
            : base(logger, httpContextAccessor)
        {
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
            _locationHoursRepository = locationHoursRepository
                ?? throw new ArgumentNullException(nameof(locationHoursRepository));
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _locationRepository.GeAllLocationsAsync();
        }

        public async Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _locationRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Location> GetLocationByStubAsync(string locationStub)
        {
            var location = await _locationRepository.GetLocationByStub(locationStub);
            if (location == null)
            {
                throw new OcudaException("Location not found.");
            }
            else
            {
                return location;
            }
        }

        public async Task<Location> GetLocationByIdAsync(int locationId)
        {
            return await _locationRepository.FindAsync(locationId);
        }

        public async Task<Location> AddLocationAsync(Location location)
        {
            location.Name = location.Name?.Trim();
            await ValidateAsync(location);

            await _locationRepository.AddAsync(location);
            await _locationRepository.SaveAsync();

            return location;
        }

        public async Task<Location> EditAsync(Location location)
        {
            var currentLocation = await _locationRepository.FindAsync(location.Id);
            currentLocation.Address = location.Address;
            currentLocation.City = location.City;
            currentLocation.Zip = location.Zip;
            currentLocation.State = location.State;
            currentLocation.Country = location.Country;
            currentLocation.MapLink = location.MapLink;
            currentLocation.Name = location.Name;
            currentLocation.Stub = location.Stub;
            currentLocation.Code = location.Code;
            currentLocation.Phone = location.Phone;
            currentLocation.Description = location.Description;
            currentLocation.Facebook = location.Facebook;
            currentLocation.EventLink = location.EventLink;
            currentLocation.SubscriptionLink = location.SubscriptionLink;
            currentLocation.DisplayGroupId = location.DisplayGroupId;

            await ValidateAsync(currentLocation);

            _locationRepository.Update(currentLocation);
            await _locationRepository.SaveAsync();
            return currentLocation;
        }

        public async Task<Location> EditAlwaysOpenAsync(Location location)
        {
            var currentLocation = await _locationRepository.FindAsync(location.Id);
            currentLocation.IsAlwaysOpen = location.IsAlwaysOpen;

            await ValidateAsync(currentLocation);

            _locationRepository.Update(currentLocation);
            await _locationRepository.SaveAsync();
            return currentLocation;
        }

        public async Task<List<LocationDayGrouping>> GetFormattedWeeklyHoursAsync(int locationId)
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

            var formattedDayGroupings = new List<LocationDayGrouping>();
            foreach (var (DaysOfWeek, OpenTime, CloseTime) in dayGroupings)
            {
                var days = GetFormattedDayGroupings(DaysOfWeek);

                var openTime = new StringBuilder(OpenTime.ToString("%h"));
                if (OpenTime.Minute != 0)
                {
                    openTime.Append(OpenTime.ToString(":mm"));
                }
                openTime.Append(OpenTime.ToString(" tt").ToLower());

                var closeTime = new StringBuilder(CloseTime.ToString("%h"));
                if (CloseTime.Minute != 0)
                {
                    closeTime.Append(CloseTime.ToString(":mm"));
                }
                closeTime.Append(CloseTime.ToString(" tt").ToLower());

                formattedDayGroupings.Add(new LocationDayGrouping
                {
                    Days = days,
                    Time = $"{openTime} {ndash} {closeTime}"
                });
            }

            if (closedDays.Count > 0)
            {
                var formattedClosedDays = GetFormattedDayGroupings(closedDays);
                formattedDayGroupings.Add(new LocationDayGrouping
                {
                    Days = formattedClosedDays,
                    Time = "Closed"
                });
            }

            return formattedDayGroupings;
        }

        public async Task DeleteAsync(int id)
        {
            var locationHours = await _locationHoursRepository.GetLocationHoursByLocationId(id);
            _locationHoursRepository.RemoveRange(locationHours);

            var location = await _locationRepository.FindAsync(id);
            _locationRepository.Remove(location);
            await _locationRepository.SaveAsync();
        }

        private string GetFormattedDayGroupings(List<DayOfWeek> days)
        {
            var dayFormatter = new DateTimeFormatInfo();
            if (days.Count == 1)
            {
                return dayFormatter.GetAbbreviatedDayName(days[0]);
            }
            else
            {
                var firstDay = days[0];
                var lastDay = days.Last();

                if (days.Count == lastDay - firstDay + 1)
                {
                    return $"{dayFormatter.GetAbbreviatedDayName(firstDay)}{ndash}{dayFormatter.GetAbbreviatedDayName(lastDay)}";
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

        private async Task ValidateAsync(Location location)
        {
            if (await _locationRepository.IsDuplicateNameAsync(location))
            {
                throw new OcudaException($"Location Name '{location.Name}' already exists.");
            }
            if (await _locationRepository.IsDuplicateStubAsync(location))
            {
                throw new OcudaException($"Location Stub '{location.Stub}' already exists.");
            }
        }
    }
}
