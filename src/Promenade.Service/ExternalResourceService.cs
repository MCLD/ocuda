using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class ExternalResourceService : BaseService<ExternalResourceService>
    {
        private readonly IOcudaCache _cache;
        private readonly IExternalResourceRepository _externalResourceRepository;

        public ExternalResourceService(ILogger<ExternalResourceService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IExternalResourceRepository externalResourceRepository)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _externalResourceRepository = externalResourceRepository
                ?? throw new ArgumentNullException(nameof(externalResourceRepository));
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(bool forceReload)
        {
            return await GetAllAsync(null, forceReload);
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type,
            bool forceReload)
        {
            var cacheKey = Utility.Keys.Cache.PromExternalResources;
            ICollection<ExternalResource> resources = null;
            if (!forceReload)
            {
                resources = await _cache
                    .GetObjectFromCacheAsync<ICollection<ExternalResource>>(cacheKey);
            }

            if (resources == null)
            {
                resources = await _externalResourceRepository.GetAllAsync(type);

                await _cache.SaveToCacheAsync(cacheKey, resources, null, CacheSlidingExpiration);
            }

            return resources;
        }
    }
}