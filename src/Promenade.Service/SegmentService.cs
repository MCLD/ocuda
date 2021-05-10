﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class SegmentService : BaseService<SegmentService>
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;

        public SegmentService(ILogger<SegmentService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository,
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
                segment = await GetObjectFromCacheAsync<Segment>(_cache, segmentCacheKey);
            }

            if (segment == null)
            {
                segment = await _segmentRepository.GetActiveAsync(segmentId);
                await SaveToCacheAsync(_cache, segmentCacheKey, segment, cachePagesInHours);
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

                string segmentTextCacheKey = null;

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
                        segmentText = await GetObjectFromCacheAsync<SegmentText>(
                            _cache,
                            segmentTextCacheKey);
                    }

                    if (segmentText == null)
                    {
                        segmentText = await _segmentTextRepository
                            .GetByIdsAsync(currentLangaugeId, segmentId);

                        await SaveToCacheAsync(_cache,
                            segmentTextCacheKey,
                            segmentText,
                            cachePagesInHours);
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
                        segmentText = await GetObjectFromCacheAsync<SegmentText>(
                            _cache,
                            segmentTextCacheKey);
                    }

                    if (segmentText == null)
                    {
                        segmentText = await _segmentTextRepository
                            .GetByIdsAsync(defaultLanguageId, segmentId);

                        await SaveToCacheAsync(_cache,
                            segmentTextCacheKey,
                            segmentText,
                            cachePagesInHours);
                    }
                }
            }

            return segmentText;
        }
    }
}