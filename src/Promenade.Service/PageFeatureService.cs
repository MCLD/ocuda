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
    public class PageFeatureService : BaseService<PageFeatureService>
    {
        private const string ImagesFilePath = "images";
        private const string PageFeaturesFilePath = "PageFeatures";

        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly IPageFeatureItemRepository _pageFeatureItemRepository;
        private readonly IPageFeatureItemTextRepository _pageFeatureItemTextRepository;
        private readonly IPageFeatureRepository _pageFeatureRepository;
        private readonly IPageFeatureTemplateRepository _pageFeatureTemplateRepository;
        private readonly IPathResolverService _pathResolver;

        public PageFeatureService(ILogger<PageFeatureService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IPathResolverService pathResolver,
            IPageFeatureItemRepository pageFeatureItemRepository,
            IPageFeatureItemTextRepository pageFeatureItemTextRepository,
            IPageFeatureRepository pageFeatureRepository,
            IPageFeatureTemplateRepository pageFeatureTemplateRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
            _pageFeatureItemRepository = pageFeatureItemRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureItemRepository));
            _pageFeatureItemTextRepository = pageFeatureItemTextRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureItemTextRepository));
            _pageFeatureRepository = pageFeatureRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureRepository));
            _pageFeatureTemplateRepository = pageFeatureTemplateRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureTemplateRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<PageFeature> GetByIdAsync(int featureId, bool forceReload)
        {
            PageFeature feature = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string featureCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromPageFeature,
                featureId);

            if (cachePagesInHours > 0 && !forceReload)
            {
                feature = await GetFromCacheAsync<PageFeature>(_cache, featureCacheKey);
            }

            if (feature == null)
            {
                feature = await _pageFeatureRepository.FindAsync(featureId);

                if (feature != null)
                {
                    feature.Items = await _pageFeatureItemRepository
                        .GetActiveForPageFeatureAsync(feature.Id);

                    foreach (var item in feature.Items)
                    {
                        item.PageFeature = null;
                    }
                }

                await SaveToCacheAsync(_cache, featureCacheKey, feature, cachePagesInHours);
            }

            if (feature != null)
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

                var invalidItems = new List<PageFeatureItem>();

                foreach (var item in feature.Items)
                {
                    if (currentLanguageId.HasValue)
                    {
                        var featureItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromPageFeatureItemText,
                        currentLanguageId,
                        item.Id);

                        if (cachePagesInHours > 0 && !forceReload)
                        {
                            item.PageFeatureItemText = await GetFromCacheAsync<PageFeatureItemText>(
                                _cache,
                                featureItemTextCacheKey);
                        }

                        if (item.PageFeatureItemText == null)
                        {
                            item.PageFeatureItemText = await _pageFeatureItemTextRepository
                                .GetByIdsAsync(item.Id, currentLanguageId.Value);

                            await SaveToCacheAsync(_cache,
                                featureItemTextCacheKey,
                                item.PageFeatureItemText,
                                cachePagesInHours);
                        }
                    }

                    if (item.PageFeatureItemText == null)
                    {
                        var featureItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                            Utility.Keys.Cache.PromPageFeatureItemText,
                            defaultLanguageId,
                            item.Id);

                        if (cachePagesInHours > 0 && !forceReload)
                        {
                            item.PageFeatureItemText = await GetFromCacheAsync<PageFeatureItemText>(
                                _cache,
                                featureItemTextCacheKey);
                        }

                        if (item.PageFeatureItemText == null)
                        {
                            item.PageFeatureItemText = await _pageFeatureItemTextRepository
                                .GetByIdsAsync(item.Id, defaultLanguageId);

                            await SaveToCacheAsync(_cache,
                                featureItemTextCacheKey,
                                item.PageFeatureItemText,
                                cachePagesInHours);
                        }
                    }

                    if (item.PageFeatureItemText == null)
                    {
                        invalidItems.Add(item);
                    }
                }

                foreach (var item in invalidItems)
                {
                    feature.Items.Remove(item);
                }
            }
            return feature;
        }

        public string GetPageFeatureFilePath(string filename)
        {
            return _pathResolver.GetPublicContentUrl(ImagesFilePath, PageFeaturesFilePath,
                filename);
        }

        public async Task<PageFeatureTemplate> GetTemplateForPageLayoutAsync(int id)
        {
            return await _pageFeatureTemplateRepository.GetForPageLayoutAsync(id);
        }
    }
}