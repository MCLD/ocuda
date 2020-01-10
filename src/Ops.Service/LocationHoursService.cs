using System;
using System.Collections.Generic;
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
        private readonly ILocationHoursRepository _locationHoursRepository;

        public LocationHoursService(ILogger<LocationHoursService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILocationHoursRepository locationHoursRepository)
            : base(logger, httpContextAccessor)
        {
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
                currentLocationHour.OpenTime = hour.OpenTime;
                currentLocationHour.CloseTime = hour.CloseTime;

                _locationHoursRepository.Update(currentLocationHour);
                await _locationHoursRepository.SaveAsync();
            }
            return locationHours;
        }

        private async Task ValidateAsync(LocationHours locationHour)
        {
            if (await _locationHoursRepository.IsDuplicateDayAsync(locationHour))
            {
                throw new OcudaException($"Location Hours for that day '{locationHour.DayOfWeek}' already exists.");
            }
        }
    }
}
