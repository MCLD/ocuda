using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class LocationHoursService : ILocationHoursService
    {
        private readonly ILogger<LocationHoursService> _logger;
        private readonly ILocationHoursRepository _locationHoursRepository;

        public LocationHoursService(ILogger<LocationHoursService> logger,
            ILocationHoursRepository locationHoursRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _locationHoursRepository = locationHoursRepository
                ?? throw new ArgumentNullException(nameof(locationHoursRepository));
        }

        public async Task<List<LocationHours>> GetLocationHoursByIdAsync(int locationId)
        {
            return await _locationHoursRepository.GetLocationHoursByLocationId(locationId);
        }

        public async Task<LocationHours> AddLocationHoursAsync(LocationHours locationHours)
        {
            try
            {
                    await ValidateAsync(locationHours);
                    await _locationHoursRepository.AddAsync(locationHours);
                    await _locationHoursRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }

                return locationHours;
        }

        public async Task<List<LocationHours>> EditAsync(List<LocationHours> locationHours)
        {
            try
            {
                foreach (var hour in locationHours)
                {
                    var currentLocationHour = await _locationHoursRepository.FindAsync(hour.Id);
                    currentLocationHour.Open = hour.Open;
                    currentLocationHour.OpenTime = hour.OpenTime;
                    currentLocationHour.CloseTime = hour.CloseTime;
                    await ValidateAsync(currentLocationHour);
                    _locationHoursRepository.Update(currentLocationHour);
                    await _locationHoursRepository.SaveAsync();
                }
                return locationHours;
            }
            catch (OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _locationHoursRepository.Remove(id);
                await _locationHoursRepository.SaveAsync();
            }
            catch(OcudaException ex)
            {
                _logger.LogError(ex, "Could not delete location", ex.Message);
                throw new OcudaException(ex.Message);
            }
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
