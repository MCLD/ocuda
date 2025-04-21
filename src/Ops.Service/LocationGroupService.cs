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
    public class LocationGroupService : BaseService<LocationGroupService>, ILocationGroupService
    {
        private readonly ILocationGroupRepository _locationGroupRepository;

        public LocationGroupService(ILogger<LocationGroupService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILocationGroupRepository locationGroupRepository)
            : base(logger, httpContextAccessor)
        {
            _locationGroupRepository = locationGroupRepository
                ?? throw new ArgumentNullException(nameof(locationGroupRepository));
        }

        public async Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location)
        {
            return await _locationGroupRepository.GetLocationGroupsByLocationAsync(location);
        }

        public async Task<LocationGroup> AddLocationGroupAsync(LocationGroup locationGroup)
        {
            await ValidateAsync(locationGroup);
            await _locationGroupRepository.AddAsync(locationGroup);
            await _locationGroupRepository.SaveAsync();
            return locationGroup;
        }

        public async Task<LocationGroup> GetByIdsAsync(int groupId, int locationId)
        {
            return await _locationGroupRepository.GetByIdsAsync(groupId, locationId);
        }

        public async Task<LocationGroup> EditAsync(LocationGroup locationGroup)
        {
            var currentLocationGroup = await _locationGroupRepository.GetByIdsAsync(
                locationGroup.GroupId, locationGroup.LocationId);

            currentLocationGroup.DisplayOrder = locationGroup.DisplayOrder;

            _locationGroupRepository.Update(currentLocationGroup);
            await _locationGroupRepository.SaveAsync();
            return currentLocationGroup;
        }

        public async Task DeleteAsync(int groupId, int locationId)
        {
            var locationGroup = await _locationGroupRepository.GetByIdsAsync(groupId, locationId);
            _locationGroupRepository.Remove(locationGroup);
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
