using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class DigitalDisplayCleanupService
        : BaseService<DigitalDisplayCleanupService>, IDigitalDisplayCleanupService
    {
        private readonly IOcudaCache _cache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDigitalDisplayService _digitalDisplayService;

        public DigitalDisplayCleanupService(IDateTimeProvider dateTimeProvider,
            IDigitalDisplayService digitalDisplayService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DigitalDisplayCleanupService> logger,
            IOcudaCache cache) : base(logger, httpContextAccessor)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _digitalDisplayService = digitalDisplayService
                ?? throw new ArgumentNullException(nameof(digitalDisplayService));
        }

        public async Task CleanupSlidesAsync()
        {
            string cacheKey = Utility.Keys.Cache.OpsCleanupSlides;
            var clear = await _cache.GetStringFromCache(cacheKey);
            if (string.IsNullOrEmpty(clear))
            {
                var now = _dateTimeProvider.Now;
                await _cache.SaveToCacheAsync(cacheKey, now.ToString("O"), 24);
                var stopwatch = Stopwatch.StartNew();
                var slideCount = await _digitalDisplayService
                    .ClearExpiredAssetsAsync(now.AddDays(-14));
                if (slideCount > 0)
                {
                    _logger.LogInformation("Cleared {SlideCount} expired slides in {ElapsedMs} ms",
                        slideCount,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}