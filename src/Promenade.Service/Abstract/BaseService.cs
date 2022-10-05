using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service.Abstract
{
    public abstract class BaseService<TService> : Utility.Services.OcudaBaseService<TService>
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        protected BaseService(ILogger<TService> logger,
            IDateTimeProvider dateTimeProvider)
            : base(logger)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));

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
        /// Get an adjusted cache duration for items that have an end date or a replacement item
        /// </summary>
        /// <param name="cacheSpan">The default cache span for this type of item</param>
        /// <param name="nextItemStart">The end date of this item or next item's start date</param>
        /// <returns>The lower of the two values</returns>
        protected TimeSpan GetCacheDuration(TimeSpan cacheSpan, DateTime nextItemStart)
        {
            if (nextItemStart == default)
            {
                return cacheSpan;
            }

            var nextUpIn = nextItemStart - _dateTimeProvider.Now;
            return nextUpIn.Ticks < cacheSpan.Ticks
                ? nextUpIn
                : cacheSpan;
        }

        /// <summary>
        /// Returns a list of language ids in the preferred order for content.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="languageService"></param>
        /// <returns>A list of language ids in the preferred order.</returns>
        /// <exception cref="ArgumentNullException">A required argument was not specified.</exception>
        protected async Task<IEnumerable<int>> GetCurrentDefaultLanguageIdAsync(
            IHttpContextAccessor httpContextAccessor,
            LanguageService languageService)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            if (languageService == null)
            {
                throw new ArgumentNullException(nameof(languageService));
            }

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
    }
}