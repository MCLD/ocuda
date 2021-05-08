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
            var cachedJson = await GetStringFromCache(cache, cacheKey);

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
                    await cache?.RemoveAsync(cacheKey);
                }
            }
            return null;
        }

        protected async Task<int?> GetIntFromCacheAsync(IDistributedCache cache, string cacheKey)
        {
            if (cache == null || string.IsNullOrEmpty(cacheKey))
            {
                return null;
            }

            var cachedValue = await cache.GetAsync(cacheKey);

            if (cachedValue?.Length > 0)
            {
                _logger.LogTrace("Cache hit for {CacheKey}", cacheKey);

                try
                {
                    return BitConverter.ToInt32(cachedValue);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    _logger.LogWarning(ex,
                        "Error converting int with key {CacheKey} from cache: {ErrorMessage}",
                        cacheKey,
                        ex.Message);
                    await cache.RemoveAsync(cacheKey);
                }
            }
            return null;
        }

        protected async Task<string> GetStringFromCache(IDistributedCache cache, string cacheKey)
        {
            if (cache == null || string.IsNullOrEmpty(cacheKey))
            {
                return null;
            }

            return await cache.GetStringAsync(cacheKey);
        }

        protected async Task SaveIntToCacheAsync(IDistributedCache cache,
            string cacheKey,
            int item,
            TimeSpan expireIn)
        {
            await SaveIntToCacheInternalAsync(cache, cacheKey, item, expireIn);
        }

        protected async Task SaveStringToCacheAsync(IDistributedCache cache,
            string cacheKey,
            string item,
            int cacheForHours)
        {
            await SaveStringToCacheAsync(cache,
                cacheKey,
                item,
                TimeSpan.FromHours(cacheForHours));
        }

        protected async Task SaveStringToCacheAsync(IDistributedCache cache,
            string cacheKey,
            string item,
            TimeSpan expireIn)
        {
            await SaveStringToCacheInternalAsync(cache, cacheKey, item, expireIn);
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            int cacheForHours) where T : class
        {
            if (cacheForHours > 0)
            {
                await SaveToCacheAsync<T>(cache,
                    cacheKey,
                    item,
                    TimeSpan.FromHours(cacheForHours));
            }
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            TimeSpan expireIn) where T : class
        {
            if (string.IsNullOrEmpty(cacheKey) || item == null)
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

        private async Task SaveIntToCacheInternalAsync(IDistributedCache cache,
            string cacheKey,
            int item,
            TimeSpan expireIn)
        {
            if (cache == null || string.IsNullOrEmpty(cacheKey))
            {
                return;
            }

            var bytes = BitConverter.GetBytes(item);

            await cache.SetAsync(cacheKey,
                BitConverter.GetBytes(item),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expireIn
                });

            _logger.LogDebug("Cache miss for {CacheKey}, caching int: {Value} as {Bytes} for {CacheTimeSpan}",
                cacheKey,
                item,
                bytes,
                expireIn);
        }

        private async Task SaveStringToCacheInternalAsync(IDistributedCache cache,
            string cacheKey,
            string item,
            TimeSpan expireIn)
        {
            if (string.IsNullOrEmpty(cacheKey) || item == null)
            {
                return;
            }

            await cache.SetStringAsync(cacheKey,
                item,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expireIn
                });

            _logger.LogDebug("Cache miss for {CacheKey}, caching {Type}: {Length} characters for {CacheTimeSpan}",
                cacheKey,
                "string",
                item.Length,
                expireIn);
        }
    }
}