using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class LocationHoursService : BaseService<LocationHoursService>, ILocationHoursService
    {
        private readonly ILocationHoursOverrideRepository _locationHoursOverrideRepository;
        private readonly ILocationHoursRepository _locationHoursRepository;

        public LocationHoursService(ILogger<LocationHoursService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILocationHoursOverrideRepository locationHoursOverrideRepository,
            ILocationHoursRepository locationHoursRepository)
            : base(logger, httpContextAccessor)
        {
            _locationHoursOverrideRepository = locationHoursOverrideRepository
                ?? throw new ArgumentNullException(nameof(locationHoursOverrideRepository));
            _locationHoursRepository = locationHoursRepository
                ?? throw new ArgumentNullException(nameof(locationHoursRepository));
        }

        public async Task<List<LocationHours>> GetLocationHoursByIdAsync(int locationId)
        {
            return await _locationHoursRepository.GetLocationHoursByLocationId(locationId);
        }

        public async Task<LocationHours> AddLocationHoursAsync(LocationHours locationHours)
        {
            await ValidateAsync(locationHours);

            await _locationHoursRepository.AddAsync(locationHours);
            await _locationHoursRepository.SaveAsync();
            return locationHours;
        }

        public async Task<List<LocationHours>> EditAsync(List<LocationHours> locationHours)
        {
            foreach (var hour in locationHours)
            {
                var currentLocationHour = await _locationHoursRepository
                    .GetByIdsAsync(hour.DayOfWeek, hour.LocationId);

                currentLocationHour.Open = hour.Open;

                if (hour.Open)
                {
                    currentLocationHour.OpenTime = hour.OpenTime;
                    currentLocationHour.CloseTime = hour.CloseTime;
                }
                else
                {
                    currentLocationHour.OpenTime = null;
                    currentLocationHour.CloseTime = null;
                }

                _locationHoursRepository.Update(currentLocationHour);
            }
            await _locationHoursRepository.SaveAsync();

            return locationHours;
        }

        public async Task<ICollection<LocationHoursOverride>> GetLocationHoursOverrideByIdAsync(
            int locationId)
        {
            return await _locationHoursOverrideRepository.GetByLocationIdAsync(locationId);
        }

        public async Task<LocationHoursOverride> AddLocationHoursOverrideAsync(
            LocationHoursOverride hoursOverride)
        {
            hoursOverride.Reason = hoursOverride.Reason?.Trim();
            if (!hoursOverride.Open)
            {
                hoursOverride.OpenTime = null;
                hoursOverride.CloseTime = null;
            }

            await ValidateOverrideAsync(hoursOverride);

            await _locationHoursOverrideRepository.AddAsync(hoursOverride);
            await _locationHoursOverrideRepository.SaveAsync();
            return hoursOverride;
        }

        public async Task<LocationHoursOverride> EditLocationHoursOverrideAsync(
            LocationHoursOverride hoursOverride)
        {
            var currentOverride = await _locationHoursOverrideRepository
                .FindAsync(hoursOverride.Id);

            currentOverride.Reason = hoursOverride?.Reason;
            currentOverride.Date = hoursOverride.Date;
            currentOverride.Open = hoursOverride.Open;
            if (hoursOverride.Open)
            {
                currentOverride.OpenTime = hoursOverride.OpenTime;
                currentOverride.CloseTime = hoursOverride.CloseTime;
            }
            else
            {
                currentOverride.OpenTime = null;
                currentOverride.CloseTime = null;
            }

            await ValidateOverrideAsync(hoursOverride);

            _locationHoursOverrideRepository.Update(currentOverride);
            await _locationHoursOverrideRepository.SaveAsync();

            return currentOverride;
        }

        public async Task DeleteLocationsHoursOverrideAsync(int id)
        {
            var hoursOverride = await _locationHoursOverrideRepository.FindAsync(id);
            _locationHoursOverrideRepository.Remove(hoursOverride);
            await _locationHoursOverrideRepository.SaveAsync();
        }

        private async Task ValidateAsync(LocationHours locationHour)
        {
            if (await _locationHoursRepository.IsDuplicateDayAsync(locationHour))
            {
                throw new OcudaException($"Location Hours for that day '{locationHour.DayOfWeek}' already exists.");
            }
        }

        private async Task ValidateOverrideAsync(LocationHoursOverride hoursOverride)
        {
            var conflictingOverrides = await _locationHoursOverrideRepository
                .GetConflictingOverrideDatesAsync(hoursOverride);
            if (conflictingOverrides.Count > 0)
            {
                if (conflictingOverrides.Count >= 2)
                {
                    throw new OcudaException("Overrides on that date already exist for multiple locations.");
                }
                else
                {
                    throw new OcudaException($"An override on that date already exists for {conflictingOverrides.First().Location?.Name ?? "All Locations"}.");
                }
            }
        }
    }
}
