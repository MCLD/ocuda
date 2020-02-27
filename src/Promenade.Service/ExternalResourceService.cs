﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Service
{
    public class ExternalResourceService : BaseService<ExternalResourceService>
    {
        private readonly IDistributedCache _cache;
        private readonly IExternalResourceRepository _externalResourceRepository;

        public ExternalResourceService(ILogger<ExternalResourceService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
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
            long start = Stopwatch.GetTimestamp();
            var cacheKey = Utility.Keys.Cache.PromExternalResources;
            ICollection<ExternalResource> resources = null;
            if (!forceReload)
            {
                string cachedResources = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedResources))
                {
                    try
                    {
                        resources = JsonSerializer
                            .Deserialize<ICollection<ExternalResource>>(cachedResources);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex,
                            "Error deserializing external resources from cache: {ErrorMessage}",
                            ex.Message);
                    }
                }
            }

            if (resources == null)
            {
                resources = await _externalResourceRepository.GetAllAsync(type);

                string resToCache = JsonSerializer.Serialize(resources);

                await _cache.SetStringAsync(cacheKey,
                    resToCache,
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = CacheSlidingExpiration
                    });
                _logger.LogDebug("Cache miss for {CacheKey}, caching {Length} characters in {Elapsed} ms",
                    cacheKey,
                    resToCache.Length,
                    (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
            }

            return resources;
        }
    }
}
