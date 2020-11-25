using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Ocuda.Utility.Services
{
    public abstract class OcudaBaseService<TService>
    {
        protected readonly ILogger<TService> _logger;

        protected OcudaBaseService(ILogger<TService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async Task<T> GetFromCacheAsync<T>(IDistributedCache cache, string cacheKey)
            where T : class
        {
            if (cache == null || string.IsNullOrEmpty(cacheKey))
            {
                return null;
            }

            string cachedJson = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                _logger.LogTrace("Cache hit for {CacheKey}", cacheKey);

                try
                {
                    return JsonSerializer.Deserialize<T>(cachedJson);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex,
                        "Error deserializing {Type} with key {CacheKey} from cache: {ErrorMessage}",
                        typeof(T),
                        cacheKey,
                        ex.Message);
                    await cache.RemoveAsync(cacheKey);
                }
            }
            return null;
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            int? cacheDurationHours) where T : class
        {
            if (cacheDurationHours == null
                || cacheDurationHours < 1
                || string.IsNullOrEmpty(cacheKey)
                || item == null)
            {
                return;
            }

            await SaveToCacheAsync<T>(cache,
                cacheKey,
                item,
                TimeSpan.FromHours((int)cacheDurationHours));
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            TimeSpan? expireIn) where T : class
        {
            if (!expireIn.HasValue
                || string.IsNullOrEmpty(cacheKey)
                || item == null)
            {
                return;
            }

            string itemJson = JsonSerializer.Serialize(item);

            await cache.SetStringAsync(cacheKey,
                itemJson,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expireIn
                });

            _logger.LogDebug("Cache miss for {CacheKey}, caching {Type}: {Length} characters for {CacheTimeSpan}",
                cacheKey,
                typeof(T),
                itemJson.Length,
                expireIn);
        }
    }
}
