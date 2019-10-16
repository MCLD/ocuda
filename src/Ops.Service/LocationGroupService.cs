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

        public async Task<LocationGroup> AddLocationGroupAsync(LocationGroup locationGroup)
        {
            await _locationGroupRepository.AddAsync(locationGroup);
            await _locationGroupRepository.SaveAsync();
            return locationGroup;
        }

        public async Task<LocationGroup> GetLocationGroupByIdAsync(int locationgroupId)
        {
            return await _locationGroupRepository.FindAsync(locationgroupId);
        }

        public async Task<LocationGroup> EditAsync(LocationGroup locationGroup)
        {
            var currentLocationGroup = await _locationGroupRepository.FindAsync(locationGroup.Id);
            await ValidateAsync(currentLocationGroup);
            _locationGroupRepository.Update(currentLocationGroup);
            await _locationGroupRepository.SaveAsync();
            return currentLocationGroup;
        }

        public async Task DeleteAsync(int id)
        {
            _locationGroupRepository.Remove(id);
            await _locationGroupRepository.SaveAsync();
        }

        private async Task ValidateAsync(LocationGroup locationGroup)
        {
            if (await _locationGroupRepository.IsDuplicateAsync(locationGroup))
            {
                throw new OcudaException("LocationGroup already exists.");
            }
        }
    }
}
