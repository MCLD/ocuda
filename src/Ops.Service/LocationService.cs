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
            return await _locationRepository.GetLocationByStub(locationStub);
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
                if (currentLocation != null)
                {
                    if (!(currentLocation.Address.Equals(location.Address) && currentLocation.City.Equals(location.City)
                    && currentLocation.State.Equals(location.State) && currentLocation.Zip.Equals(location.Zip)
                    && currentLocation.Country.Equals(location.Country)))
                    {
                        currentLocation.Address = location.Address;
                        currentLocation.City = location.City;
                        currentLocation.Zip = location.Zip;
                        currentLocation.State = location.State;
                        currentLocation.Country = location.Country;
                        currentLocation.MapLink = location.MapLink;
                    }
                    if (!currentLocation.Name.Equals(location.Name))
                    {
                        currentLocation.Name = location.Name;
                    }
                    if (!currentLocation.Stub.Equals(location.Stub))
                    {
                        currentLocation.Stub = location.Stub;
                    }
                    if (!currentLocation.Code.Equals(location.Code))
                    {
                        currentLocation.Code = location.Code;
                    }
                    if (!currentLocation.Phone.Equals(location.Phone))
                    {
                        currentLocation.Phone = location.Phone;
                    }
                    if (!currentLocation.Description.Equals(location.Description))
                    {
                        currentLocation.Description = location.Description;
                    }
                    if (!currentLocation.Facebook.Equals(location.Facebook))
                    {
                        currentLocation.Facebook = location.Facebook;
                    }
                    if (!currentLocation.EventLink.Equals(location.EventLink))
                    {
                        currentLocation.EventLink = location.EventLink;
                    }
                    if (!currentLocation.SubscriptionLink.Equals(location.SubscriptionLink))
                    {
                        currentLocation.SubscriptionLink = location.SubscriptionLink;
                    }
                }
                _locationRepository.Update(currentLocation);
                await _locationRepository.SaveAsync();
                return await _locationRepository.FindAsync(currentLocation.Id);
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
            if (await _locationRepository.IsDuplicateAsync(location))
            {
                throw new OcudaException($"Location type '{location.Name}' already exists.");
            }
        }
    }
}
