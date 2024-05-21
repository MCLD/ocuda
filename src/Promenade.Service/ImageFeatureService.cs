using System;
using System.Collections.Generic;
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
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class ImageFeatureService : BaseService<ImageFeatureService>
    {
        private const string FeaturesFilePath = "features";
        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFeatureItemRepository _imageFeatureItemRepository;
        private readonly IImageFeatureItemTextRepository _imageFeatureItemTextRepository;
        private readonly IImageFeatureRepository _imageFeatureRepository;
        private readonly IImageFeatureTemplateRepository _imageFeatureTemplateRepository;
        private readonly LanguageService _languageService;
        private readonly IPathResolverService _pathResolver;

        public ImageFeatureService(ILogger<ImageFeatureService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IImageFeatureItemRepository imageFeatureItemRepository,
            IImageFeatureItemTextRepository imageFeatureItemTextRepository,
            IImageFeatureRepository imageFeatureRepository,
            IImageFeatureTemplateRepository imageFeatureTemplateRepository,
            LanguageService languageService,
            IPathResolverService pathResolver)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
            _imageFeatureItemRepository = imageFeatureItemRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureItemRepository));
            _imageFeatureItemTextRepository = imageFeatureItemTextRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureItemTextRepository));
            _imageFeatureRepository = imageFeatureRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureRepository));
            _imageFeatureTemplateRepository = imageFeatureTemplateRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureTemplateRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<ImageFeature> GetByIdAsync(int imageFeatureId, bool forceReload)
        {
            ImageFeature imageFeature = null;

            bool cacheImageFeature = false;
            var cachePageSpan = GetPageCacheSpan(_config);

            string imageFeatureCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromImageFeature,
                imageFeatureId);

            if (cachePageSpan.HasValue && !forceReload)
            {
                imageFeature = await _cache.GetObjectFromCacheAsync<ImageFeature>(imageFeatureCacheKey);
            }

            if (imageFeature == null)
            {
                imageFeature = await _imageFeatureRepository.FindAsync(imageFeatureId);

                if (imageFeature != null)
                {
                    imageFeature.Items = await _imageFeatureItemRepository
                        .GetActiveForImageFeatureAsync(imageFeature.Id);

                    foreach (var item in imageFeature.Items)
                    {
                        item.ImageFeature = null;
                    }
                }

                if (cachePageSpan.HasValue)
                {
                    cacheImageFeature = true;
                }
            }

            if (imageFeature != null)
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

                var invalidItems = new List<ImageFeatureItem>();

                string languageName = await _languageService
                    .GetLanguageNameAsync(currentLanguageId ?? defaultLanguageId, forceReload);

                foreach (var item in imageFeature.Items)
                {
                    var expire = cachePageSpan;
                    if (cachePageSpan.HasValue && item.EndDate.HasValue)
                    {
                        expire = GetCacheDuration(cachePageSpan.Value, item.EndDate.Value);
                    }

                    if (currentLanguageId.HasValue)
                    {
                        var itemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                            Utility.Keys.Cache.PromImageFeatureItemText,
                            currentLanguageId,
                            item.Id);

                        if (expire.HasValue && !forceReload)
                        {
                            item.ImageFeatureItemText = await _cache
                                .GetObjectFromCacheAsync<ImageFeatureItemText>(itemTextCacheKey);
                        }

                        if (item.ImageFeatureItemText == null)
                        {
                            item.ImageFeatureItemText = await _imageFeatureItemTextRepository
                                .GetByIdsAsync(item.Id, currentLanguageId.Value);

                            if (item.ImageFeatureItemText != null)
                            {
                                item.ImageFeatureItemText.Filepath = _pathResolver
                                    .GetPublicContentLink(ImagesFilePath,
                                        languageName,
                                        FeaturesFilePath,
                                        item.ImageFeatureItemText.Filename);

                                if (item.ImageFeatureItemText
                                    .Filepath
                                    .EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                                {
                                    var path = _pathResolver
                                        .GetPublicContentFilePath(item.ImageFeatureItemText.Filename,
                                            ImagesFilePath,
                                            languageName,
                                            FeaturesFilePath);
                                    if (System.IO.File.Exists(path))
                                    {
                                        item.ImageFeatureItemText.FileContent =
                                            System.IO.File.ReadAllText(path);
                                    }
                                    else
                                    {
                                        invalidItems.Add(item);
                                    }
                                }

                                if (expire.HasValue && !invalidItems.Contains(item))
                                {
                                    await _cache.SaveToCacheAsync(itemTextCacheKey,
                                        item.ImageFeatureItemText,
                                        expire.Value);
                                }
                            }
                        }
                    }

                    if (item.ImageFeatureItemText == null && !invalidItems.Contains(item))
                    {
                        var itemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                            Utility.Keys.Cache.PromImageFeatureItemText,
                            defaultLanguageId,
                            item.Id);

                        if (expire.HasValue && !forceReload)
                        {
                            item.ImageFeatureItemText = await _cache
                                .GetObjectFromCacheAsync<ImageFeatureItemText>(itemTextCacheKey);
                        }

                        item.ImageFeatureItemText ??= await _imageFeatureItemTextRepository
                                .GetByIdsAsync(item.Id, defaultLanguageId);

                        if (item.ImageFeatureItemText != null)
                        {
                            item.ImageFeatureItemText.Filepath = _pathResolver
                                .GetPublicContentLink(ImagesFilePath,
                                    languageName,
                                    FeaturesFilePath,
                                    item.ImageFeatureItemText.Filename);

                            if (expire.HasValue)
                            {
                                await _cache.SaveToCacheAsync(itemTextCacheKey,
                                    item.ImageFeatureItemText,
                                    expire.Value);
                            }
                        }
                        else
                        {
                            invalidItems.Add(item);
                        }
                    }
                }

                foreach (var item in invalidItems)
                {
                    _logger.LogWarning("Invalid image feature item: {ItemInfo}", item.Name);
                    imageFeature.Items.Remove(item);
                }

                if (cachePageSpan.HasValue && cacheImageFeature)
                {
                    var expire = cachePageSpan.Value;
                    var earliestExpiration = imageFeature.Items.Where(_ => _.EndDate != null
                            && _.EndDate != default
                            && _.EndDate > _dateTimeProvider.Now)
                        .Min(_ => _.EndDate);

                    if (earliestExpiration.HasValue)
                    {
                        expire = GetCacheDuration(cachePageSpan.Value, earliestExpiration.Value);
                        if (expire != cachePageSpan.Value)
                        {
                            _logger.LogInformation("Shortening webslide {Name} ({ItemId}) cache to {CacheForTime}, due to item expiration at {EndDate}",
                                imageFeature.Name,
                                imageFeature.Id,
                                expire,
                                earliestExpiration.Value);
                        }
                    }

                    await _cache.SaveToCacheAsync(imageFeatureCacheKey, imageFeature, expire);
                }
            }
            return imageFeature;
        }

        public async Task<ImageFeatureTemplate> GetTemplateForPageLayoutAsync(int id)
        {
            return await _imageFeatureTemplateRepository.GetForPageLayoutAsync(id);
        }
    }
}