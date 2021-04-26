using System;
using System.Collections.Generic;
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
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class WebslideService : BaseService<WebslideService>
    {
        private const string ImagesFilePath = "images";
        private const string WebslidesFilePath = "Webslides";

        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPathResolverService _pathResolver;
        private readonly IWebslideItemRepository _webslideItemRepository;
        private readonly IWebslideItemTextRepository _webslideItemTextRepository;
        private readonly IWebslideRepository _webslideRepository;
        private readonly LanguageService _languageService;

        public WebslideService(ILogger<WebslideService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IPathResolverService pathResolver,
            IWebslideItemRepository webslideItemRepository,
            IWebslideItemTextRepository webslideItemTextRepository,
            IWebslideRepository webslideRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
            _webslideItemRepository = webslideItemRepository
                ?? throw new ArgumentNullException(nameof(webslideItemRepository));
            _webslideItemTextRepository = webslideItemTextRepository
                ?? throw new ArgumentNullException(nameof(webslideItemTextRepository));
            _webslideRepository = webslideRepository
                ?? throw new ArgumentNullException(nameof(webslideRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<Webslide> GetByIdAsync(int webslideId, bool forceReload)
        {
            Webslide webslide = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string webslideCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromWebslide,
                webslideId);

            if (cachePagesInHours != null && !forceReload)
            {
                webslide = await GetFromCacheAsync<Webslide>(_cache, webslideCacheKey);
            }

            if (webslide == null)
            {
                webslide = await _webslideRepository.FindAsync(webslideId);

                if (webslide != null)
                {
                    webslide.Items = await _webslideItemRepository
                        .GetActiveForWebslideAsync(webslide.Id);

                    foreach (var item in webslide.Items)
                    {
                        item.Webslide = null;
                    }
                }

                await SaveToCacheAsync(_cache, webslideCacheKey, webslide, cachePagesInHours);
            }

            if (webslide != null)
            {
                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;

                int defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                int? currentLanguageId = null;
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    currentLanguageId = await _languageService
                        .GetLanguageIdAsync(currentCultureName);
                }

                var invalidItems = new List<WebslideItem>();

                foreach (var item in webslide.Items)
                {
                    if (currentLanguageId.HasValue)
                    {
                        var webslideItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromWebslideItemText,
                        currentLanguageId,
                        item.Id);

                        if (cachePagesInHours != null && !forceReload)
                        {
                            item.WebslideItemText = await GetFromCacheAsync<WebslideItemText>(
                                _cache,
                                webslideItemTextCacheKey);
                        }

                        if (item.WebslideItemText == null)
                        {
                            item.WebslideItemText = await _webslideItemTextRepository
                                .GetByIdsAsync(item.Id, currentLanguageId.Value);

                            await SaveToCacheAsync(_cache,
                                webslideItemTextCacheKey,
                                item.WebslideItemText,
                                cachePagesInHours);
                        }
                    }

                    if (item.WebslideItemText == null)
                    {
                        var webslideItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                            Utility.Keys.Cache.PromWebslideItemText,
                            defaultLanguageId,
                            item.Id);

                        if (cachePagesInHours != null && !forceReload)
                        {
                            item.WebslideItemText = await GetFromCacheAsync<WebslideItemText>(
                                _cache,
                                webslideItemTextCacheKey);
                        }

                        if (item.WebslideItemText == null)
                        {
                            item.WebslideItemText = await _webslideItemTextRepository
                                .GetByIdsAsync(item.Id, defaultLanguageId);

                            await SaveToCacheAsync(_cache,
                                webslideItemTextCacheKey,
                                item.WebslideItemText,
                                cachePagesInHours);
                        }
                    }

                    if (item.WebslideItemText == null)
                    {
                        invalidItems.Add(item);
                    }
                }

                foreach (var item in invalidItems)
                {
                    webslide.Items.Remove(item);
                }
            }
            return webslide;
        }

        public string GetWebslideFilePath(string filename)
        {
            return _pathResolver.GetPublicContentUrl(ImagesFilePath, WebslidesFilePath, filename);
        }
    }
}
