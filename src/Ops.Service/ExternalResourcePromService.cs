using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ExternalResourcePromService
        : BaseService<ExternalResourcePromService>, IExternalResourcePromService
    {
        private readonly IExternalResourcePromRepository _externalResourcePromRepository;

        public ExternalResourcePromService(ILogger<ExternalResourcePromService> logger,
            IHttpContextAccessor httpContextAccessor,
            IExternalResourcePromRepository externalResourcePromRepository)
            : base(logger, httpContextAccessor)
        {
            _externalResourcePromRepository = externalResourcePromRepository
                ?? throw new ArgumentNullException(nameof(externalResourcePromRepository));
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type)
        {
            return await _externalResourcePromRepository.GetAllAsync(type);
        }

        public async Task<DataWithCount<ICollection<ExternalResource>>> GetPaginatedListAsync(
            ExternalResourceFilter filter)
        {
            return await _externalResourcePromRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ExternalResource> AddAsync(ExternalResource resource)
        {
            resource.Name = resource.Name?.Trim();
            resource.Url = resource.Url?.Trim();

            var maxSortOrder = await _externalResourcePromRepository
                .GetMaxSortOrderAsync(resource.Type);
            if (maxSortOrder.HasValue)
            {
                resource.SortOrder = maxSortOrder.Value + 1;
            }

            await _externalResourcePromRepository.AddAsync(resource);
            await _externalResourcePromRepository.SaveAsync();

            return resource;
        }

        public async Task<ExternalResource> EditAsync(ExternalResource resource)
        {
            var currentResource = await _externalResourcePromRepository.FindAsync(resource.Id);

            currentResource.Name = resource.Name?.Trim();
            currentResource.Url = resource.Url?.Trim();

            _externalResourcePromRepository.Update(currentResource);
            await _externalResourcePromRepository.SaveAsync();

            return currentResource;
        }

        public async Task DeleteAsync(int id)
        {
            var resource = await _externalResourcePromRepository.FindAsync(id);
            _externalResourcePromRepository.Remove(resource);

            var subsequentResources = await _externalResourcePromRepository
                .GetSubsequentAsync(resource.Type, resource.SortOrder);

            if (subsequentResources.Count > 0)
            {
                subsequentResources.ForEach(_ => _.SortOrder--);
                _externalResourcePromRepository.UpdateRange(subsequentResources);
            }

            await _externalResourcePromRepository.SaveAsync();
        }

        public async Task DecreaseSortOrder(int id)
        {
            var resource = await _externalResourcePromRepository.FindAsync(id);

            if (resource.SortOrder == 0)
            {
                throw new OcudaException("Resource is already in the first position.");
            }

            var newSortOrder = resource.SortOrder - 1;

            var resourceInPosition = await _externalResourcePromRepository.GetBySortOrderAsync(
                resource.Type, newSortOrder);

            resourceInPosition.SortOrder = resource.SortOrder;

            resource.SortOrder = newSortOrder;

            _externalResourcePromRepository.Update(resource);
            _externalResourcePromRepository.Update(resourceInPosition);
            await _externalResourcePromRepository.SaveAsync();
        }

        public async Task IncreaseSortOrder(int id)
        {
            var resource = await _externalResourcePromRepository.FindAsync(id);
            var newSortOrder = resource.SortOrder + 1;

            var resourceInPosition = await _externalResourcePromRepository.GetBySortOrderAsync(
                resource.Type, newSortOrder);

            if (resourceInPosition == null)
            {
                throw new OcudaException("Resource is already in the last position.");
            }

            resourceInPosition.SortOrder = resource.SortOrder;

            resource.SortOrder = newSortOrder;

            _externalResourcePromRepository.Update(resource);
            _externalResourcePromRepository.Update(resourceInPosition);
            await _externalResourcePromRepository.SaveAsync();
        }
    }
}