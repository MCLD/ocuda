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
    public class LocationGroupService : ILocationGroupService
    {
        private readonly ILogger<LocationService> _logger;
        private readonly ILocationGroupRepository _locationGroupRepository;

        public LocationGroupService(ILogger<LocationService> logger,
            ILocationGroupRepository locationGroupRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _locationGroupRepository = locationGroupRepository
                ?? throw new ArgumentNullException(nameof(locationGroupRepository));
        }
        public async Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location)
        {
            return await _locationGroupRepository.GetLocationGroupsByLocationAsync(location);
        }
    }
}
