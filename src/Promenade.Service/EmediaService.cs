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
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class EmediaService : BaseService<EmediaService>
    {
        private readonly IOcudaCache _cache;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryTextRepository _categoryTextRepository;
        private readonly IConfiguration _config;
        private readonly IEmediaCategoryRepository _emediaCategoryRepository;
        private readonly IEmediaGroupRepository _emediaGroupRepository;
        private readonly IEmediaRepository _emediaRepository;
        private readonly IEmediaTextRepository _emediaTextRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly SegmentService _segmentService;

        public EmediaService(ILogger<EmediaService> logger,
            IDateTimeProvider dateTimeProvider,
            SegmentService segmentService,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository,
            ICategoryTextRepository categoryTextRepository,
            IConfiguration config,
            IOcudaCache cache,
            IEmediaRepository emediaRepository,
            IEmediaCategoryRepository emediaCategoryRepository,
            IEmediaGroupRepository emediaGroupRepository,
            IEmediaTextRepository emediaTextRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(categoryTextRepository);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(emediaCategoryRepository);
            ArgumentNullException.ThrowIfNull(emediaGroupRepository);
            ArgumentNullException.ThrowIfNull(emediaRepository);
            ArgumentNullException.ThrowIfNull(emediaTextRepository);
            ArgumentNullException.ThrowIfNull(httpContextAccessor);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _cache = cache;
            _categoryRepository = categoryRepository;
            _categoryTextRepository = categoryTextRepository;
            _config = config;
            _emediaCategoryRepository = emediaCategoryRepository;
            _emediaGroupRepository = emediaGroupRepository;
            _emediaRepository = emediaRepository;
            _emediaTextRepository = emediaTextRepository;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _segmentService = segmentService;
        }

        private int CachePagesInHours
        {
            get
            {
                return GetPageCacheDuration(_config);
            }
        }

        public async Task<Emedia> GetAsync(string slug)
        {
            return await _emediaRepository.Find(slug);
        }

        public async Task<ICollection<EmediaGroup>> GetGroupedEmediaAsync(bool forceReload)
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
                currentLanguageId = await _languageService.GetLanguageIdAsync(currentCultureName);
            }

            ICollection<EmediaGroup> groups = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                groups = await _cache.GetObjectFromCacheAsync<ICollection<EmediaGroup>>(
                    Utility.Keys.Cache.PromEmediaGroups);
            }

            if (groups == null || groups.Count == 0)
            {
                groups = await _emediaGroupRepository.GetAllAsync();
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.PromEmediaGroups,
                    groups,
                    CachePagesInHours);
            }

            ICollection<Emedia> emedias = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                emedias = await _cache.GetObjectFromCacheAsync<ICollection<Emedia>>(
                    Utility.Keys.Cache.PromEmedias);
            }

            if (emedias == null || emedias.Count == 0)
            {
                emedias = await _emediaRepository.GetAllAsync();
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.PromEmedias,
                    emedias,
                    CachePagesInHours);
            }

            ICollection<Category> categories = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                categories = await _cache.GetObjectFromCacheAsync<ICollection<Category>>(
                    Utility.Keys.Cache.PromCategories);
            }

            if (categories == null || categories.Count == 0)
            {
                categories = await _categoryRepository.GetAllCategories();
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.PromCategories,
                    categories,
                    CachePagesInHours);
            }

            foreach (var category in categories)
            {
                if (currentLanguageId.HasValue)
                {
                    category.CategoryText = await GetCategoryTextAsync(forceReload,
                        (int)currentLanguageId,
                        category.Id);
                }
                category.CategoryText ??= await GetCategoryTextAsync(forceReload,
                        defaultLanguageId,
                        category.Id);
            }

            ICollection<EmediaCategory> emediaCategories = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                emediaCategories = await _cache
                    .GetObjectFromCacheAsync<ICollection<EmediaCategory>>(
                        Utility.Keys.Cache.PromEmediaCategories);
            }

            if (emediaCategories == null || emediaCategories.Count == 0)
            {
                emediaCategories = await _emediaCategoryRepository.GetAllAsync();
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.PromEmediaCategories,
                    emediaCategories,
                    CachePagesInHours);
            }

            foreach (var group in groups)
            {
                group.Emedias = [.. emedias
                    .Where(_ => _.GroupId == group.Id)
                    .OrderBy(_ => _.Name)];

                if (group.SegmentId.HasValue && group.Segment == null)
                {
                    group.Segment = new Segment
                    {
                        SegmentText = await _segmentService
                        .GetSegmentTextBySegmentIdAsync((int)group.SegmentId, forceReload)
                    };
                }

                foreach (var emedia in group.Emedias)
                {
                    if (currentLanguageId.HasValue)
                    {
                        emedia.EmediaText = await GetEmediaTextAsync(forceReload,
                            (int)currentLanguageId,
                            emedia.Id);
                    }

                    emedia.EmediaText ??= await GetEmediaTextAsync(forceReload,
                            defaultLanguageId,
                            emedia.Id);

                    var thisEmediaCategoryIds = emediaCategories
                        .Where(_ => _.EmediaId == emedia.Id)
                        .Select(_ => _.CategoryId);

                    emedia.Categories
                        .AddRange([.. categories.Where(_ => thisEmediaCategoryIds.Contains(_.Id))]);
                }
            }

            return groups;
        }

        private async Task<CategoryText> GetCategoryTextAsync(bool forceReload,
            int languageId,
            int categoryId)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromCategoryText,
                languageId,
                categoryId);

            CategoryText categoryText = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                categoryText = await _cache.GetObjectFromCacheAsync<CategoryText>(cacheKey);
            }

            if (categoryText == null)
            {
                categoryText = await _categoryTextRepository.GetByIdsAsync(
                    categoryId,
                    languageId);

                if (categoryText != null)
                {
                    await _cache.SaveToCacheAsync(cacheKey,
                        categoryText,
                        CachePagesInHours);
                }
            }

            return categoryText;
        }

        private async Task<EmediaText> GetEmediaTextAsync(bool forceReload,
            int languageId,
            int emediaId)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromEmediaText,
                languageId,
                emediaId);

            EmediaText emediaText = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                emediaText = await _cache.GetObjectFromCacheAsync<EmediaText>(cacheKey);
            }

            if (emediaText == null)
            {
                emediaText = await _emediaTextRepository.GetByIdsAsync(
                    emediaId,
                    languageId);

                if (emediaText != null)
                {
                    await _cache.SaveToCacheAsync(cacheKey, emediaText, CachePagesInHours);
                }
            }

            return emediaText;
        }
    }
}