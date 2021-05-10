using System;
using System.Globalization;
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

        protected Task<bool?> GetBoolFromCacheAsync(IDistributedCache cache, string cacheKey)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            return GetBoolFromCacheInternalAsync(cache, cacheKey);
        }

        protected Task<int?> GetIntFromCacheAsync(IDistributedCache cache, string cacheKey)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            return GetIntFromCacheInternalAsync(cache, cacheKey);
        }

        protected async Task<T> GetObjectFromCacheAsync<T>(IDistributedCache cache,
            string cacheKey) where T : class
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

        protected Task<string> GetStringFromCache(IDistributedCache cache, string cacheKey)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            return GetStringFromCacheInternalAsync(cache, cacheKey);
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            int cacheForHours)
        {
            if (cacheForHours > 0)
            {
                await SaveToCacheInternalAsync(cache,
                    cacheKey,
                    item,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(cacheForHours)
                    });
            }
            else
            {
                _logger.LogWarning("Refusing to cache item {CacheKey} for less than {CacheForHours} hour(s)",
                    cacheKey,
                    cacheForHours);
            }
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            TimeSpan absoluteExpiration)
        {
            await SaveToCacheInternalAsync(cache, cacheKey, item, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration
            });
        }

        protected async Task SaveToCacheAsync<T>(IDistributedCache cache,
                    string cacheKey,
            T item,
            TimeSpan? absoluteExpiration,
            TimeSpan slidingExpiration)
        {
            await SaveToCacheInternalAsync(cache, cacheKey, item, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            });
        }

        private static async Task<string> GetStringFromCacheInternalAsync(IDistributedCache cache,
            string cacheKey)
        {
            return await cache.GetStringAsync(cacheKey);
        }

        private async Task<bool?> GetBoolFromCacheInternalAsync(IDistributedCache cache,
            string cacheKey)
        {
            var cachedValue = await cache.GetAsync(cacheKey);

            if (cachedValue?.Length > 0)
            {
                _logger.LogTrace("Cache hit for {CacheKey}", cacheKey);

                try
                {
                    return BitConverter.ToBoolean(cachedValue);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    _logger.LogWarning(ex,
                        "Error converting bool with key {CacheKey} from cache: {ErrorMessage}",
                        cacheKey,
                        ex.Message);
                    await cache.RemoveAsync(cacheKey);
                }
            }
            return null;
        }

        private async Task<int?> GetIntFromCacheInternalAsync(IDistributedCache cache,
                    string cacheKey)
        {
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

        private Task SaveToCacheInternalAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            DistributedCacheEntryOptions cacheEntryOptions)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            if (item == null)
            {
                _logger.LogError("Ignoring attempt to cache null object with key {CacheKey}", cacheKey);
                return Task.CompletedTask;
            }
            if (cacheEntryOptions == null)
            {
                throw new ArgumentNullException(nameof(cacheEntryOptions));
            }
            return SaveToCacheInternalExecuteAsync(cache, cacheKey, item, cacheEntryOptions);
        }

        private async Task SaveToCacheInternalExecuteAsync<T>(IDistributedCache cache,
            string cacheKey,
            T item,
            DistributedCacheEntryOptions cacheEntryOptions)
        {
            int length;
            string description;

            if (item is bool boolValue)
            {
                var bytes = BitConverter.GetBytes(boolValue);
                await cache.SetAsync(cacheKey, bytes, cacheEntryOptions);
                description = bytes.ToString();
                length = bytes.Length;
            }
            else if (item is int intValue)
            {
                var bytes = BitConverter.GetBytes(intValue);
                await cache.SetAsync(cacheKey, bytes, cacheEntryOptions);
                description = "bytes " + BitConverter.ToString(bytes);
                length = bytes.Length;
            }
            else
            {
                string cacheValue;
                if (item is string stringValue)
                {
                    cacheValue = stringValue;
                    description = "string";
                }
                else
                {
                    cacheValue = JsonSerializer.Serialize(item);
                    description = typeof(T).Name;
                }
                length = cacheValue.Length;
                await cache.SetStringAsync(cacheKey, cacheValue, cacheEntryOptions);
            }

            string timeSpan
                = cacheEntryOptions.AbsoluteExpirationRelativeToNow?.ToString()
                ?? cacheEntryOptions.AbsoluteExpiration?.ToString(CultureInfo.InvariantCulture)
                ?? "unspecified timespan";

            string timespanExpiration = cacheEntryOptions.SlidingExpiration.HasValue
                ? cacheEntryOptions.SlidingExpiration.Value + " sliding"
                : "absolute";

            _logger.LogDebug("Cache miss for {CacheKey}: caching {Description} (length {Length}) expires {CacheTimeSpan} ({ExpirationType})",
                cacheKey,
                description,
                length,
                timeSpan,
                timespanExpiration);
        }
    }
}