using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class SegmentService : BaseService<SegmentService>
    {
        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;
        private readonly ISegmentWrapRepository _segmentWrapRepository;

        public SegmentService(ILogger<SegmentService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository,
            ISegmentWrapRepository segmentWrapRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _segmentRepository = segmentRepository
                ?? throw new ArgumentNullException(nameof(segmentRepository));
            _segmentTextRepository = segmentTextRepository
                ?? throw new ArgumentNullException(nameof(segmentTextRepository));
            _segmentWrapRepository = segmentWrapRepository
                 ?? throw new ArgumentNullException(nameof(segmentWrapRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<SegmentText> GetSegmentTextBySegmentIdAsync(int segmentId,
            bool forceReload)
        {
            SegmentText segmentText = null;
            Segment segment = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string segmentCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromSegment,
                segmentId);

            if (cachePagesInHours > 0 && !forceReload)
            {
                segment = await _cache.GetObjectFromCacheAsync<Segment>(segmentCacheKey);
            }

            if (segment == null)
            {
                segment = await _segmentRepository.GetActiveAsync(segmentId);

                bool cached = false;
                if (segment.EndDate.HasValue)
                {
                    var timeFromNow = segment.EndDate.Value - DateTime.Now;
                    if (timeFromNow < TimeSpan.FromHours(cachePagesInHours))
                    {
                        await _cache.SaveToCacheAsync(segmentCacheKey, segment, timeFromNow);
                        cached = true;
                    }
                }

                if (!cached)
                {
                    await _cache.SaveToCacheAsync(segmentCacheKey, segment, cachePagesInHours);
                }
            }

            if (segment != null)
            {
                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;

                string segmentTextCacheKey;
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    var currentLangaugeId = await _languageService
                        .GetLanguageIdAsync(currentCultureName);

                    segmentTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromSegmentText,
                        currentLangaugeId,
                        segmentId);

                    if (cachePagesInHours > 0 && !forceReload)
                    {
                        segmentText = await _cache.GetObjectFromCacheAsync<SegmentText>(
                            segmentTextCacheKey);
                    }

                    if (segmentText == null)
                    {
                        segmentText = await _segmentTextRepository
                            .GetByIdsAsync(currentLangaugeId, segmentId);

                        if (segmentText != null)
                        {
                            await _cache.SaveToCacheAsync(segmentTextCacheKey,
                                segmentText,
                                cachePagesInHours);
                        }
                    }
                }

                if (segmentText == null)
                {
                    var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();

                    segmentTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromSegmentText,
                        defaultLanguageId,
                        segmentId);

                    if (cachePagesInHours > 0 && !forceReload)
                    {
                        segmentText = await _cache.GetObjectFromCacheAsync<SegmentText>(
                            segmentTextCacheKey);
                    }

                    if (segmentText == null)
                    {
                        segmentText = await _segmentTextRepository
                            .GetByIdsAsync(defaultLanguageId, segmentId);

                        await _cache.SaveToCacheAsync(segmentTextCacheKey,
                            segmentText,
                            cachePagesInHours);
                    }
                }
            }

            SegmentWrap segmentWrap = null;

            if (segment.SegmentWrapId.HasValue)
            {
                string segmentWrapCacheId = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromSegmentWrap,
                    segment.SegmentWrapId.Value);

                if (cachePagesInHours > 0 && !forceReload)
                {
                    segmentWrap = await _cache
                        .GetObjectFromCacheAsync<SegmentWrap>(segmentWrapCacheId);
                }

                if (segmentWrap == null)
                {
                    segmentWrap = await _segmentWrapRepository
                        .GetActiveAsync(segment.SegmentWrapId.Value);
                    await _cache
                        .SaveToCacheAsync(segmentWrapCacheId, segmentWrap, cachePagesInHours);
                }
            }

            if (segmentWrap != null && segmentText != null)
            {
                segmentText.SegmentWrapPrefix = segmentWrap.Prefix;
                segmentText.SegmentWrapSuffix = segmentWrap.Suffix;
            }

            return segmentText;
        }
    }
}