using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
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
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPageHeaderRepository _pageHeaderRepository;
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly IPageLayoutTextRepository _pageLayoutTextRepository;
        private readonly IPageRepository _pageRepository;
        private readonly LanguageService _languageService;

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

            int? currentLanguageId = null;

            if (!string.IsNullOrWhiteSpace(currentCultureName))
            {
                currentLanguageId = await _languageService.GetLanguageIdAsync(currentCultureName);
            }

            int? defaultLanguageId = null;

            Page page = null;

            var cachePagesHoursString
                = _config[Utility.Keys.Configuration.PromenadeCachePagesHours];

            int? cachePagesInHours = null;

            if (!string.IsNullOrEmpty(cachePagesHoursString)
                && int.TryParse(cachePagesHoursString, out int cacheInHours))
            {
                cachePagesInHours = cacheInHours;
            }

            if (cachePagesInHours != null && !forceReload)
            {
                if (currentLanguageId != null)
                {
                    page = await GetPageFromCacheAsync((int)currentLanguageId, type, fixedStub);
                }

                if (page == null)
                {
                    defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                    page = await GetPageFromCacheAsync((int)defaultLanguageId, type, fixedStub);
                }
            }

            if (page == null && currentLanguageId != null)
            {
                page = await _pageRepository.GetPublishedByStubAndTypeAsync(fixedStub,
                    type,
                    (int)currentLanguageId);

                if (page != null && cachePagesInHours != null)
                {
                    await SavePageToCacheAsync(cachePagesInHours ?? 1,
                        (int)currentLanguageId,
                        type,
                        fixedStub,
                        page,
                        forceReload);
                }
            }

            if (page == null)
            {
                if (defaultLanguageId == null)
                {
                    defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                }

                page = await _pageRepository.GetPublishedByStubAndTypeAsync(fixedStub,
                    type,
                    (int)defaultLanguageId);

                if (page != null && cachePagesInHours != null)
                {
                    await SavePageToCacheAsync(cachePagesInHours ?? 1,
                        (int)defaultLanguageId,
                        type,
                        fixedStub,
                        page,
                        forceReload);
                }
            }

            return page;
        }

        private async Task<Page> GetPageFromCacheAsync(int languageId, PageType type, string stub)
        {
            /// Cached page, {0} is the language id, {1} is the type, {2} is the stub
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPage,
                    languageId,
                    type,
                    stub);

            string cachedPage = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedPage))
            {
                _logger.LogTrace("Cache hit for {CacheKey}", cacheKey);

                try
                {
                    return JsonSerializer.Deserialize<Page>(cachedPage);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex,
                        "Error deserializing page {Stub} language {LanguageId} type {PageType} from cache: {ErrorMessage}",
                        stub,
                        languageId,
                        type,
                        ex.Message);
                }
            }
            return null;
        }

        private async Task SavePageToCacheAsync(int cachePagesInHours,
            int languageId,
            PageType type,
            string stub,
            Page page,
            bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPage,
                    languageId,
                    type,
                    stub);

            string pageToCache = JsonSerializer.Serialize(page);

            await _cache.SetStringAsync(cacheKey,
                pageToCache,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(cachePagesInHours)
                });

            if (forceReload)
            {
                _logger.LogDebug("Forced cache reload for {CacheKey}, caching {Length} characters",
                    cacheKey,
                    pageToCache.Length);
            }
            else
            {
                _logger.LogDebug("Cache miss for {CacheKey}, caching {Length} characters",
                    cacheKey,
                    pageToCache.Length);
            }
        }

        public async Task<PageHeader> GetHeaderByStubAndTypeAsync(string stub, PageType type)
        {
            return await _pageHeaderRepository.GetByStubAndTypeAsync(stub?.Trim(), type);
        }

        public async Task<PageLayout> GetLayoutPageByHeaderAsync(int headerId,
            bool forceReload,
            string previewIdString)
        {
            bool isPreview = false;
            int? layoutId = null;

            if (!string.IsNullOrEmpty(previewIdString) &&
                Guid.TryParse(previewIdString, out Guid previewIdGuid))
            {
                layoutId = await _pageLayoutRepository
                    .GetPreviewLayoutIdAsync(headerId, previewIdGuid);
            }

            if (layoutId.HasValue)
            {
                isPreview = true;
                forceReload = true;
            }
            else
            {
                layoutId = await _pageLayoutRepository.GetCurrentLayoutIdForHeaderAsync(headerId);
            }

            if (!layoutId.HasValue)
            {
                return null;
            }

            PageLayout pageLayout = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string layoutCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromPageLayout,
                layoutId.Value);

            if (cachePagesInHours != null && !forceReload)
            {
                pageLayout = await GetFromCacheAsync<PageLayout>(_cache, layoutCacheKey);
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

                await SaveToCacheAsync(_cache, layoutCacheKey, pageLayout, cachePagesInHours);
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

                    if (cachePagesInHours != null && !forceReload)
                    {
                        pageLayout.PageLayoutText = await GetFromCacheAsync<PageLayoutText>(_cache,
                            layoutTextCacheKey);
                    }

                    if (pageLayout.PageLayoutText == null)
                    {
                        pageLayout.PageLayoutText = await _pageLayoutTextRepository
                            .GetByIdsAsync(pageLayout.Id, currentLangaugeId);

                        await SaveToCacheAsync(_cache,
                            layoutTextCacheKey,
                            pageLayout.PageLayoutText,
                            cachePagesInHours);
                    }
                }

                if (pageLayout.PageLayoutText == null)
                {
                    var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();

                    var layoutTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromPageLayoutText,
                        defaultLanguageId,
                        pageLayout.Id);

                    if (cachePagesInHours != null && !forceReload)
                    {
                        pageLayout.PageLayoutText = await GetFromCacheAsync<PageLayoutText>(_cache,
                            layoutTextCacheKey);
                    }

                    if (pageLayout.PageLayoutText == null)
                    {
                        pageLayout.PageLayoutText = await _pageLayoutTextRepository
                            .GetByIdsAsync(pageLayout.Id, defaultLanguageId);

                        await SaveToCacheAsync(_cache,
                            layoutTextCacheKey,
                            pageLayout.PageLayoutText,
                            cachePagesInHours);
                    }
                }

                pageLayout.IsPreview = isPreview;
            }

            return pageLayout;
        }
    }
}
