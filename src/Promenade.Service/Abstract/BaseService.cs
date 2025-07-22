using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service.Abstract
{
    public abstract class BaseService<TService> : Utility.Services.OcudaBaseService<TService>
    {
        protected const string ImagesFilePath = "images";

        protected readonly IDateTimeProvider _dateTimeProvider;

        protected BaseService(ILogger<TService> logger,
            IDateTimeProvider dateTimeProvider)
            : base(logger)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);

            _dateTimeProvider = dateTimeProvider;

            CacheSlidingExpiration = new TimeSpan(1, 0, 0);
        }

        protected TimeSpan CacheSlidingExpiration { get; set; }

        protected static int GetPageCacheDuration(IConfiguration config)
        {
            if (config == null)
            {
                return 0;
            }

            var cachePagesHoursString
                = config[Utility.Keys.Configuration.PromenadeCachePagesHours];

            int? cachePagesInHours = null;

            if (!string.IsNullOrEmpty(cachePagesHoursString)
                && int.TryParse(cachePagesHoursString, out int cacheInHours))
            {
                cachePagesInHours = cacheInHours;
            }

            return cachePagesInHours ?? 0;
        }

        protected static TimeSpan? GetPageCacheSpan(IConfiguration config)
        {
            var duration = GetPageCacheDuration(config);

            if (duration < 1)
            {
                return null;
            }

            return TimeSpan.FromHours(duration);
        }

        /// <summary>
        /// Get an adjusted cache duration for items that may have a limited period for caching
        /// </summary>
        /// <param name="cacheSpan">The default cache span for this type of item</param>
        /// <param name="dates">
        /// An array of dates for new information for computation of a cache expiration
        /// <returns>The lower of the two values</returns>
        protected TimeSpan GetCacheDuration(TimeSpan cacheSpan, DateTime?[] dates)
        {
            var validDates = dates?.Where(_ => _ != null);

            if (validDates == null)
            {
                return cacheSpan;
            }

            var nextDate = validDates
                .Where(_ => _ > _dateTimeProvider.Now)
                .Order()
                .FirstOrDefault(_ => _dateTimeProvider.Now + cacheSpan > _);

            return nextDate != null
                ? nextDate.Value - _dateTimeProvider.Now
                : cacheSpan;
        }

        /// <summary>
        /// Returns a list of language ids in the preferred order for content.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="languageService"></param>
        /// <returns>A list of language ids in the preferred order.</returns>
        /// <exception cref="ArgumentNullException">A required argument was not specified.
        /// </exception>
        protected async Task<IEnumerable<int>> GetCurrentDefaultLanguageIdAsync(
            IHttpContextAccessor httpContextAccessor,
            LanguageService languageService)
        {
            ArgumentNullException.ThrowIfNull(httpContextAccessor);
            ArgumentNullException.ThrowIfNull(languageService);

            var currentCultureName = httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            int defaultLanguageId = await languageService.GetDefaultLanguageIdAsync();
            int? currentLanguageId = null;
            if (!string.IsNullOrWhiteSpace(currentCultureName))
            {
                currentLanguageId = await languageService.GetLanguageIdAsync(
                    currentCultureName);
            }

            return new[]
            {
                currentLanguageId ?? defaultLanguageId,
                defaultLanguageId
            }.Distinct();
        }

        /// <summary>
        /// <para>Get an item from cache if possible - if not, perform a database lookup.</para>
        /// <para>
        /// This method attempts to fetch an item from a cacheKey and itemId. If that fails, it
        /// uses the dbLookupAsync method to look the value up and sotres it in the cache.
        /// </para>
        /// <para>
        /// If the forceReload parameter is true it will not try to get the value from the
        /// cache at all.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The object type we are attempting to fetch/look up</typeparam>
        /// <param name="cacheKey">
        ///     The cacheKey with two tokens, {0} for item id and {1} for language id
        /// </param>
        /// <param name="itemId">The id of the item we are looking for.</param>
        /// <param name="cacheForHours">Number of hours to cache items</param>
        /// <param name="cache">A distributed cache provider that implements IOcudaCache</param>
        /// <param name="forceReload">Whether we should skip checking cache altogether</param>
        /// <param name="dbLookupAsync">
        ///     A method for performing the database lookup, only takes one parameter: item id
        /// <returns>The object if it can be found in cache or the database</returns>
        /// <exception cref="OcudaException">
        ///     Thrown if the cacheKey is not valid (not enough or too many replacement tokens)
        /// </exception>
        protected async Task<T> GetFromCacheDatabaseAsync<T>(string cacheKey,
            int itemId,
            int cacheForHours,
            IOcudaCache cache,
            bool forceReload,
            Func<int, Task<T>> dbLookupAsync) where T : class
        {
            ArgumentNullException.ThrowIfNull(cacheKey);
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(dbLookupAsync);

            if (!cacheKey.Contains("{0}", StringComparison.OrdinalIgnoreCase))
            {
                throw new OcudaException("Invalid cache key, must contain {0} for item id.");
            }

            if (cacheKey.Contains("{1}", StringComparison.OrdinalIgnoreCase))
            {
                throw new OcudaException("Invalid cache key, must only contain one replacement token: {0} for item id.");
            }

            string populatedCacheKey = string.Format(CultureInfo.InvariantCulture,
                cacheKey,
                itemId);

            T item = null;

            if (!forceReload)
            {
                item = await cache.GetObjectFromCacheAsync<T>(populatedCacheKey);
            }

            if (item == null)
            {
                item = await dbLookupAsync(itemId);
                if (item != null)
                {
                    await cache.SaveToCacheAsync(populatedCacheKey, item, cacheForHours);
                }
            }

            return item;
        }

        /// <summary>
        /// <para>Get an item from cache if possible - if not, perform a database lookup.</para>
        /// <para>
        /// This method attempts, in order:
        /// 1. Fetch item using supplied cacheKey and itemId from cache in the current language.
        /// 2. If it cannot be found, fetch the item using the itemId and current language from the
        ///    database using the passed-in method.
        /// 3. If it cannot be found then repeat the same process for the default language.
        /// </para>
        /// <para>
        /// If the forceReload parameter is true it will not try to get the value from the
        /// cache at all.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The object type we are attempting to fetch/look up</typeparam>
        /// <param name="cacheKey">
        ///     The cacheKey with two tokens, {0} for item id and {1} for language id
        /// </param>
        /// <param name="itemId">The id of the item we are looking for.</param>
        /// <param name="languageIds">Language ids to look up in preferred order</param>
        /// <param name="cacheForHours">Number of hours to cache items</param>
        /// <param name="cache">A distributed cache provider that implements IOcudaCache</param>
        /// <param name="forceReload">Whether we should skip checking cache altogether</param>
        /// <param name="dbLookupAsync">
        ///     A method for performing the database lookup, should only take two parameters in
        ///     order: item id, language id</param>
        /// <returns>The object if it can be found in cache or the database</returns>
        /// <exception cref="OcudaException">
        ///     Thrown if the cacheKey is not valid (not enough or too many replacement tokens)
        /// </exception>
        protected async Task<T> GetFromCacheDatabaseAsync<T>(string cacheKey,
            int itemId,
            IEnumerable<int> languageIds,
            int cacheForHours,
            IOcudaCache cache,
            bool forceReload,
            Func<int, int, Task<T>> dbLookupAsync) where T : class
        {
            ArgumentNullException.ThrowIfNull(cacheKey);
            ArgumentNullException.ThrowIfNull(languageIds);
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(dbLookupAsync);

            if (!cacheKey.Contains("{0}", StringComparison.OrdinalIgnoreCase)
                || !cacheKey.Contains("{1}", StringComparison.OrdinalIgnoreCase))
            {
                throw new OcudaException("Invalid cache key, must contain {0} for item id and {1} for language id.");
            }

            if (cacheKey.Contains("{2}", StringComparison.OrdinalIgnoreCase))
            {
                throw new OcudaException("Invalid cache key, must only contain two replacement tokens: {0} for item id and {1} for language id.");
            }

            T item = null;

            foreach (var languageId in languageIds)
            {
                string populatedCacheKey = string.Format(CultureInfo.InvariantCulture,
                    cacheKey,
                    languageId,
                    itemId);

                if (cacheForHours > 0 && !forceReload)
                {
                    item = await cache.GetObjectFromCacheAsync<T>(populatedCacheKey);
                }

                if (item == null)
                {
                    item = await dbLookupAsync(itemId, languageId);

                    if (item != null)
                    {
                        await cache.SaveToCacheAsync(populatedCacheKey, item, cacheForHours);
                    }
                }

                if (item != null)
                {
                    return item;
                }
            }
            return item;
        }
    }
}