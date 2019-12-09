using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
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
            locationGroup.CreatedAt = DateTime.Now;
            locationGroup.CreatedBy = GetCurrentUserId();

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

            currentLocationGroup.UpdatedAt = DateTime.Now;
            currentLocationGroup.UpdatedBy = GetCurrentUserId();

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
