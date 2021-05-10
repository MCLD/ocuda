using System;
using System.Globalization;
using System.Linq;
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
    public class PageService : BaseService<PageService>
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly IPageHeaderRepository _pageHeaderRepository;
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly IPageLayoutTextRepository _pageLayoutTextRepository;
        private readonly IPageRepository _pageRepository;

        public PageService(ILogger<PageService> logger,
            IConfiguration config,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor,
            IPageHeaderRepository pageHeaderRepository,
            IPageLayoutRepository pageLayoutRepository,
            IPageLayoutTextRepository pageLayoutTextRepository,
            IPageRepository pageRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pageHeaderRepository = pageHeaderRepository
                ?? throw new ArgumentNullException(nameof(pageHeaderRepository));
            _pageLayoutRepository = pageLayoutRepository
                ?? throw new ArgumentNullException(nameof(pageLayoutRepository));
            _pageLayoutTextRepository = pageLayoutTextRepository
                ?? throw new ArgumentNullException(nameof(pageLayoutTextRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<Page> GetContentPageByStubAndType(string stub,
            PageType type,
            bool forceReload)
        {
            var fixedStub = stub?.Trim();

            var currentCultureName = _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            Page page = null;

            var cachePagesHoursString
                = _config[Utility.Keys.Configuration.PromenadeCachePagesHours];

            int? cachePagesInHours = null;

            if (!string.IsNullOrEmpty(cachePagesHoursString)
                && int.TryParse(cachePagesHoursString, out int cacheInHours))
            {
                cachePagesInHours = cacheInHours;
            }

            if (currentCultureName != i18n.Culture.DefaultName)
            {
                int currentLanguageId
                    = await _languageService.GetLanguageIdAsync(currentCultureName);

                if (cachePagesInHours > 0 && !forceReload)
                {
                    page = await GetPageFromCacheAsync(currentLanguageId, type, fixedStub);
                }

                if (page == null)
                {
                    page = await _pageRepository.GetPublishedByStubAndTypeAsync(fixedStub,
                        type,
                        currentLanguageId);

                    if (page != null && cachePagesInHours > 0)
                    {
                        await SavePageToCacheAsync(cachePagesInHours ?? 1,
                            currentLanguageId,
                            type,
                            fixedStub,
                            page);
                    }
                }
            }

            if (page == null)
            {
                int defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();

                if (cachePagesInHours > 0 && !forceReload)
                {
                    page = await GetPageFromCacheAsync(defaultLanguageId, type, fixedStub);
                }

                if (page == null)
                {
                    page = await _pageRepository.GetPublishedByStubAndTypeAsync(fixedStub,
                        type,
                        defaultLanguageId);

                    if (page != null && cachePagesInHours > 0)
                    {
                        await SavePageToCacheAsync(cachePagesInHours ?? 1,
                            defaultLanguageId,
                            type,
                            fixedStub,
                            page);
                    }
                }
            }

            return page;
        }

        public async Task<PageHeader> GetHeaderByStubAndTypeAsync(string stub,
            PageType type,
            bool forceReload)
        {
            var cachePagesInHours = GetPageCacheDuration(_config);
            string headerCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromPageHeader,
                stub,
                type);

            PageHeader pageHeader = null;

            if (cachePagesInHours > 0 && !forceReload)
            {
                pageHeader = await GetObjectFromCacheAsync<PageHeader>(_cache, headerCacheKey);
            }

            if (pageHeader == null)
            {
                pageHeader = await _pageHeaderRepository.GetByStubAndTypeAsync(stub?.Trim(), type);

                if (cachePagesInHours > 0 && pageHeader != null)
                {
                    await SaveToCacheAsync(_cache,
                        headerCacheKey,
                        pageHeader,
                        cachePagesInHours);
                }
            }

            return pageHeader;
        }

        public async Task<PageLayout> GetLayoutPageByHeaderAsync(int headerId,
            bool forceReloadRequested,
            string previewIdString)
        {
            bool forceReload = forceReloadRequested;
            bool isPreview = false;
            int? layoutId = null;

            if (!string.IsNullOrEmpty(previewIdString) &&
                Guid.TryParse(previewIdString, out Guid previewIdGuid))
            {
                layoutId = await _pageLayoutRepository
                    .GetPreviewLayoutIdAsync(headerId, previewIdGuid);
            }

            var cacheSpan = GetPageCacheSpan(_config);

            if (layoutId.HasValue)
            {
                isPreview = true;
                forceReload = true;
            }
            else
            {
                string currentLayoutIdCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPageCurrentLayoutId,
                    headerId);

                if (cacheSpan.HasValue && !forceReload)
                {
                    layoutId = await GetIntFromCacheAsync(_cache, currentLayoutIdCacheKey);
                }

                if (!layoutId.HasValue)
                {
                    layoutId = await _pageLayoutRepository
                        .GetCurrentLayoutIdForHeaderAsync(headerId);

                    if (layoutId.HasValue && cacheSpan.HasValue)
                    {
                        var nextUp = await _pageLayoutRepository.GetNextStartDate(headerId);
                        if (nextUp.HasValue)
                        {
                            var earliestSpan = GetCacheDuration(cacheSpan.Value, nextUp.Value);
                            if (earliestSpan.HasValue && earliestSpan.Value != cacheSpan.Value)
                            {
                                _logger.LogInformation("Shortening layout id {LayoutId} cache to {CacheForTime}, next layout activates at {StartDate}",
                                    layoutId,
                                    earliestSpan,
                                    nextUp.Value);
                                cacheSpan = earliestSpan.Value;
                            }
                        }

                        if (cacheSpan.HasValue)
                        {
                            await SaveToCacheAsync(_cache,
                                currentLayoutIdCacheKey,
                                layoutId.Value,
                                cacheSpan.Value);
                        }
                    }
                }
            }

            if (!layoutId.HasValue)
            {
                return null;
            }

            PageLayout pageLayout = null;

            string layoutCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromPageLayout,
                layoutId.Value);

            if (cacheSpan.HasValue && !forceReload)
            {
                pageLayout = await GetObjectFromCacheAsync<PageLayout>(_cache, layoutCacheKey);
            }

            if (pageLayout == null)
            {
                pageLayout = await _pageLayoutRepository.GetIncludingChildrenAsync(layoutId.Value);
                if (pageLayout != null)
                {
                    pageLayout.Items = pageLayout.Items?.OrderBy(_ => _.Order).ToList();
                    foreach (var item in pageLayout.Items)
                    {
                        item.PageLayout = null;
                    }
                }

                await SaveToCacheAsync(_cache, layoutCacheKey, pageLayout, cacheSpan.Value);
            }

            if (pageLayout != null)
            {
                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;

                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    var currentLangaugeId = await _languageService
                        .GetLanguageIdAsync(currentCultureName);

                    var layoutTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromPageLayoutText,
                        currentLangaugeId,
                        pageLayout.Id);

                    if (cacheSpan.HasValue && !forceReload)
                    {
                        pageLayout.PageLayoutText = await GetObjectFromCacheAsync<PageLayoutText>(_cache,
                            layoutTextCacheKey);
                    }

                    if (pageLayout.PageLayoutText == null)
                    {
                        pageLayout.PageLayoutText = await _pageLayoutTextRepository
                            .GetByIdsAsync(pageLayout.Id, currentLangaugeId);

                        if (cacheSpan.HasValue)
                        {
                            await SaveToCacheAsync(_cache,
                                layoutTextCacheKey,
                                pageLayout.PageLayoutText,
                                cacheSpan.Value);
                        }
                    }
                }

                if (pageLayout.PageLayoutText == null)
                {
                    var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();

                    var layoutTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromPageLayoutText,
                        defaultLanguageId,
                        pageLayout.Id);

                    if (cacheSpan.HasValue && !forceReload)
                    {
                        pageLayout.PageLayoutText = await GetObjectFromCacheAsync<PageLayoutText>(_cache,
                            layoutTextCacheKey);
                    }

                    if (pageLayout.PageLayoutText == null)
                    {
                        pageLayout.PageLayoutText = await _pageLayoutTextRepository
                            .GetByIdsAsync(pageLayout.Id, defaultLanguageId);

                        if (cacheSpan.HasValue)
                        {
                            await SaveToCacheAsync(_cache,
                                layoutTextCacheKey,
                                pageLayout.PageLayoutText,
                                cacheSpan.Value);
                        }
                    }
                }

                pageLayout.IsPreview = isPreview;
            }

            return pageLayout;
        }

        private async Task<Page> GetPageFromCacheAsync(int languageId, PageType type, string stub)
        {
            /// Cached page, {0} is the language id, {1} is the type, {2} is the stub
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPage,
                    languageId,
                    type,
                    stub);

            return await GetObjectFromCacheAsync<Page>(_cache, cacheKey);
        }

        private async Task SavePageToCacheAsync(int cachePagesInHours,
            int languageId,
            PageType type,
            string stub,
            Page page)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPage,
                    languageId,
                    type,
                    stub);

            await SaveToCacheAsync(_cache, cacheKey, page, cachePagesInHours);
        }
    }
}