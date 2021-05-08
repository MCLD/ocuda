using System;
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

        protected TimeSpan? GetCacheDuration(TimeSpan cacheSpan, DateTime nextItemStart)
        {
            if (cacheSpan.TotalSeconds < 60 || nextItemStart == default)
            {
                return null;
            }

            var nextUpIn = nextItemStart - _dateTimeProvider.Now;
            return nextUpIn.Ticks < cacheSpan.Ticks
                ? nextUpIn
                : cacheSpan;
        }
    }
}