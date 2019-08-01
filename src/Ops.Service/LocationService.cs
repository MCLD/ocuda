using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILogger<LocationService> logger,
            ILocationRepository locationRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _locationRepository.GeAllLocationsAsync();
        }

        public async Task<Location> GetLocationByStubAsync(string locationStub)
        {
            return await _locationRepository.GetLocationByStub(locationStub);
        }

        public async Task<Location> AddAsync(Location location)
        {
            location.Name = location.Name?.Trim();

            await ValidateAsync(location);

            await _locationRepository.AddAsync(location);
            await _locationRepository.SaveAsync();

            return location;
        }

        public async Task<Location> EditAsync(Location location)
        {
            var currentlocation = await _locationRepository.FindAsync(location.Id);

            currentlocation.Name = location.Name?.Trim();

            await ValidateAsync(currentlocation);

            _locationRepository.Update(currentlocation);
            await _locationRepository.SaveAsync();

            return currentlocation;
        }

        public async Task DeleteAsync(int id)
        {
            _locationRepository.Remove(id);
            await _locationRepository.SaveAsync();
        }

        private async Task ValidateAsync(Location location)
        {
            if (await _locationRepository.IsDuplicateAsync(location))
            {
                throw new OcudaException($"Location type '{location.Name}' already exists.");
            }
        }
    }
}
