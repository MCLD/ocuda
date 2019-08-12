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
        public async Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _locationRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Location> GetLocationByStubAsync(string locationStub)
        {
            var location = await _locationRepository.GetLocationByStub(locationStub);
            if(location == null)
            {
                throw new OcudaException($"Location not found.");
            }
            else
            {
                return await _locationRepository.GetLocationByStub(locationStub);
            }
            
        }

        public async Task<Location> GetLocationByIdAsync(int locationId)
        {
            return await _locationRepository.FindAsync(locationId);
        }

        public async Task<Location> AddLocationAsync(Location location)
        {
            try
            {
                location.Name = location.Name?.Trim();
                await ValidateAsync(location);

                await _locationRepository.AddAsync(location);
                await _locationRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }

                return location;
        }

        public async Task<Location> EditAsync(Location location)
        {
            try
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
                await ValidateAsync(currentLocation);
                _locationRepository.Update(currentLocation);
                await _locationRepository.SaveAsync();
                return currentLocation;
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
                _locationRepository.Remove(id);
                await _locationRepository.SaveAsync();
            }
            catch(OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }
        }
        private async Task ValidateAsync(Location location)
        {
            if (await _locationRepository.IsDuplicateIdAsync(location))
            {
                throw new OcudaException($"Location Id already exists.");
            }
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
