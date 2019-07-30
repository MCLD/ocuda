using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;

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
    }
}
