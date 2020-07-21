using System;
using System.Globalization;
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
        private readonly IPageRepository _pageRepository;
        private readonly LanguageService _languageService;

        public PageService(ILogger<PageService> logger,
            IConfiguration config,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor,
            IPageRepository pageRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<Page> GetByStubAndType(string stub, PageType type, bool forceReload)
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
    }
}
