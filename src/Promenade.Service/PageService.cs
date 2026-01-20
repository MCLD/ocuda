using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class PageService : BaseService<PageService>
    {
        private readonly IOcudaCache _cache;
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
            IOcudaCache cache,
            IHttpContextAccessor httpContextAccessor,
            IPageHeaderRepository pageHeaderRepository,
            IPageLayoutRepository pageLayoutRepository,
            IPageLayoutTextRepository pageLayoutTextRepository,
            IPageRepository pageRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(httpContextAccessor);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(pageHeaderRepository);
            ArgumentNullException.ThrowIfNull(pageLayoutRepository);
            ArgumentNullException.ThrowIfNull(pageLayoutTextRepository);
            ArgumentNullException.ThrowIfNull(pageRepository);

            _cache = cache;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _pageHeaderRepository = pageHeaderRepository;
            _pageLayoutRepository = pageLayoutRepository;
            _pageLayoutTextRepository = pageLayoutTextRepository;
            _pageRepository = pageRepository;
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
                pageHeader = await _cache.GetObjectFromCacheAsync<PageHeader>(headerCacheKey);
            }

            if (pageHeader == null)
            {
                pageHeader = await _pageHeaderRepository.GetByStubAndTypeAsync(stub?.Trim(), type);

                if (cachePagesInHours > 0 && pageHeader != null)
                {
                    await _cache.SaveToCacheAsync(headerCacheKey,
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
            else if (!string.IsNullOrEmpty(previewIdString))
            {
                _logger.LogWarning("Unable to find preview {PreviewId} for header {HeaderId}",
                    previewIdString,
                    headerId);
                var oex = new OcudaException($"Unable to find preview id {previewIdString} for header {headerId}");
                oex.Data[nameof(StatusCodes)] = StatusCodes.Status405MethodNotAllowed;
                throw oex;
            }
            else
            {
                string currentLayoutIdCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPageCurrentLayoutId,
                    headerId);

                if (cacheSpan.HasValue && !forceReload)
                {
                    layoutId = await _cache.GetIntFromCacheAsync(currentLayoutIdCacheKey);
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
                            var earliestSpan = GetCacheDuration(cacheSpan.Value, [nextUp.Value]);
                            if (earliestSpan != cacheSpan.Value)
                            {
                                _logger.LogInformation("Shortening layout id {LayoutId} cache to {CacheForTime}, next layout activates at {StartDate}",
                                    layoutId,
                                    earliestSpan,
                                    nextUp.Value);
                                cacheSpan = earliestSpan;
                            }
                        }

                        if (cacheSpan.HasValue)
                        {
                            await _cache.SaveToCacheAsync(currentLayoutIdCacheKey,
                                layoutId.Value,
                                cacheSpan.Value);
                        }
                    }
                }
            }

            if (!layoutId.HasValue)
            {
                _logger.LogWarning("Unable to find layout for header {HeaderId}", headerId);
                var oex = new OcudaException($"Unable to find layout for header {headerId}");
                oex.Data[nameof(StatusCodes)] = StatusCodes.Status404NotFound;
                throw oex;
            }

            PageLayout pageLayout = null;

            string layoutCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromPageLayout,
                layoutId.Value);

            if (cacheSpan.HasValue && !forceReload)
            {
                pageLayout = await _cache.GetObjectFromCacheAsync<PageLayout>(layoutCacheKey);
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

                if (cacheSpan.HasValue)
                {
                    await _cache.SaveToCacheAsync(layoutCacheKey, pageLayout, cacheSpan.Value);
                }
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
                    var currentLanguageId = await _languageService
                        .GetLanguageIdAsync(currentCultureName);

                    var layoutTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromPageLayoutText,
                        currentLanguageId,
                        pageLayout.Id);

                    if (cacheSpan.HasValue && !forceReload)
                    {
                        pageLayout.PageLayoutText = await _cache
                            .GetObjectFromCacheAsync<PageLayoutText>(layoutTextCacheKey);
                    }

                    if (pageLayout.PageLayoutText == null)
                    {
                        pageLayout.PageLayoutText = await _pageLayoutTextRepository
                            .GetByIdsAsync(pageLayout.Id, currentLanguageId);

                        if (cacheSpan.HasValue && pageLayout?.PageLayoutText != null)
                        {
                            await _cache.SaveToCacheAsync(layoutTextCacheKey,
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
                        pageLayout.PageLayoutText = await _cache
                            .GetObjectFromCacheAsync<PageLayoutText>(layoutTextCacheKey);
                    }

                    if (pageLayout.PageLayoutText == null)
                    {
                        pageLayout.PageLayoutText = await _pageLayoutTextRepository
                            .GetByIdsAsync(pageLayout.Id, defaultLanguageId);

                        if (cacheSpan.HasValue && pageLayout?.PageLayoutText != null)
                        {
                            await _cache.SaveToCacheAsync(layoutTextCacheKey,
                                pageLayout.PageLayoutText,
                                cacheSpan.Value);
                        }
                    }
                }

                pageLayout.IsPreview = isPreview;
            }

            return pageLayout;
        }

        public async Task<string> GetStubByHeaderIdTypeAsync(int headerId,
            PageType type,
            bool forceReload)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromPageHeaderSlug,
                headerId,
                type);

            PageHeader header = null;

            if (!forceReload)
            {
                header = await _cache.GetObjectFromCacheAsync<PageHeader>(cacheKey);
            }

            if (header == null)
            {
                header = await _pageHeaderRepository.GetByIdAndTypeAsync(headerId, type);

                var cachePagesInHours = GetPageCacheDuration(_config);

                if (header != null && cachePagesInHours > 0)
                {
                    await _cache.SaveToCacheAsync(cacheKey, header, cachePagesInHours);
                }
            }

            return header.Stub?.Trim();
        }

        private async Task<Page> GetPageFromCacheAsync(int languageId, PageType type, string stub)
        {
            /// Cached page, {0} is the language id, {1} is the type, {2} is the stub
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromPage,
                    languageId,
                    type,
                    stub);

            return await _cache.GetObjectFromCacheAsync<Page>(cacheKey);
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

            await _cache.SaveToCacheAsync(cacheKey, page, cachePagesInHours);
        }
    }
}