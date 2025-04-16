using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ExternalResourceService
        : BaseService<ExternalResourceService>, IExternalResourceService
    {
        private readonly IExternalResourceRepository _externalResourceRepository;

        public ExternalResourceService(ILogger<ExternalResourceService> logger,
            IHttpContextAccessor httpContextAccessor,
            IExternalResourceRepository externalResourceRepository)
            : base(logger, httpContextAccessor)
        {
            _externalResourceRepository = externalResourceRepository
                ?? throw new ArgumentNullException(nameof(externalResourceRepository));
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type)
        {
            return await _externalResourceRepository.GetAllAsync(type);
        }

        public async Task<DataWithCount<ICollection<ExternalResource>>> GetPaginatedListAsync(
            ExternalResourceFilter filter)
        {
            return await _externalResourceRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ExternalResource> AddAsync(ExternalResource resource)
        {
            resource.CreatedAt = DateTime.Now;
            resource.CreatedBy = GetCurrentUserId();
            resource.Name = resource.Name?.Trim();
            resource.Url = resource.Url?.Trim();

            var maxSortOrder = await _externalResourceRepository
                .GetMaxSortOrderAsync(resource.Type);
            if (maxSortOrder.HasValue)
            {
                resource.SortOrder = maxSortOrder.Value + 1;
            }

            await _externalResourceRepository.AddAsync(resource);
            await _externalResourceRepository.SaveAsync();

            return resource;
        }

        public async Task<ExternalResource> EditAsync(ExternalResource resource)
        {
            var currentResource = await _externalResourceRepository.FindAsync(resource.Id);

            currentResource.Name = resource.Name?.Trim();
            currentResource.Url = resource.Url?.Trim();
            currentResource.UpdatedAt = DateTime.Now;
            currentResource.UpdatedBy = GetCurrentUserId();

            _externalResourceRepository.Update(currentResource);
            await _externalResourceRepository.SaveAsync();

            return currentResource;
        }

        public async Task DeleteAsync(int id)
        {
            var resource = await _externalResourceRepository.FindAsync(id);
            _externalResourceRepository.Remove(resource);

            var subsequentResources = await _externalResourceRepository
                .GetSubsequentAsync(resource.Type, resource.SortOrder);

            if (subsequentResources.Count > 0)
            {
                subsequentResources.ForEach(_ => _.SortOrder--);
                _externalResourceRepository.UpdateRange(subsequentResources);
            }

            await _externalResourceRepository.SaveAsync();
        }

        public async Task DecreaseSortOrder(int id)
        {
            var resource = await _externalResourceRepository.FindAsync(id);

            if (resource.SortOrder == 0)
            {
                throw new OcudaException("Resource is already in the first position.");
            }

            var newSortOrder = resource.SortOrder - 1;

            var resourceInPosition = await _externalResourceRepository.GetBySortOrderAsync(
                resource.Type, newSortOrder);

            var now = DateTime.Now;
            var currentUserId = GetCurrentUserId();

            resourceInPosition.SortOrder = resource.SortOrder;
            resourceInPosition.UpdatedAt = now;
            resourceInPosition.UpdatedBy = currentUserId;

            resource.SortOrder = newSortOrder;
            resource.UpdatedAt = now;
            resource.UpdatedBy = currentUserId;

            _externalResourceRepository.Update(resource);
            _externalResourceRepository.Update(resourceInPosition);
            await _externalResourceRepository.SaveAsync();
        }

        public async Task IncreaseSortOrder(int id)
        {
            var resource = await _externalResourceRepository.FindAsync(id);
            var newSortOrder = resource.SortOrder + 1;

            var resourceInPosition = await _externalResourceRepository.GetBySortOrderAsync(
                resource.Type, newSortOrder);

            if (resourceInPosition == null)
            {
                throw new OcudaException("Resource is already in the last position.");
            }

            var now = DateTime.Now;
            var currentUserId = GetCurrentUserId();

            resourceInPosition.SortOrder = resource.SortOrder;
            resourceInPosition.UpdatedAt = now;
            resourceInPosition.UpdatedBy = currentUserId;

            resource.SortOrder = newSortOrder;
            resource.UpdatedAt = now;
            resource.UpdatedBy = currentUserId;

            _externalResourceRepository.Update(resource);
            _externalResourceRepository.Update(resourceInPosition);
            await _externalResourceRepository.SaveAsync();
        }
    }
}