using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service.Abstract
{
    public abstract class BaseService<TService>
    {
        protected TimeSpan CacheSlidingExpiration { get; set; }

        protected readonly ILogger<TService> _logger;
        protected readonly IDateTimeProvider _dateTimeProvider;

        protected BaseService(ILogger<TService> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            CacheSlidingExpiration = new TimeSpan(1, 0, 0);
        }

        protected static int? GetPageCacheDuration(IConfiguration config)
        {
            if (config == null)
            {
                return null;
            }

            var cachePagesHoursString
                = config[Utility.Keys.Configuration.PromenadeCachePagesHours];

            int? cachePagesInHours = null;

            if (!string.IsNullOrEmpty(cachePagesHoursString)
                && int.TryParse(cachePagesHoursString, out int cacheInHours))
            {
                cachePagesInHours = cacheInHours;
            }

            return cachePagesInHours;
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
            int? cacheDuration) where T : class
        {
            if (cacheDuration == null || string.IsNullOrEmpty(cacheKey) || item == null)
            {
                return;
            }

            string itemJson = JsonSerializer.Serialize(item);

            await cache.SetStringAsync(cacheKey,
                itemJson,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours((int)cacheDuration)
                });

            _logger.LogDebug("Cache miss for {CacheKey}, caching {Type}: {Length} characters",
                cacheKey,
                typeof(T),
                itemJson.Length);
        }
    }
}
