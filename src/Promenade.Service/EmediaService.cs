using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class EmediaService(ILogger<EmediaService> logger,
        IDateTimeProvider dateTimeProvider,
        ICategoryRepository categoryRepository,
        ICategoryTextRepository categoryTextRepository,
        IConfiguration config,
        IEmediaAccessRepository emediaAccessRepository,
        IEmediaCategoryRepository emediaCategoryRepository,
        IEmediaGroupRepository emediaGroupRepository,
        IEmediaRepository emediaRepository,
        IEmediaSubjectRepository emediaSubjectRepository,
        IEmediaTextRepository emediaTextRepository,
        IHttpContextAccessor httpContextAccessor,
        IOcudaCache cache,
        LanguageService languageService,
        SegmentService segmentService) : BaseService<EmediaService>(logger, dateTimeProvider)
    {
        private readonly IOcudaCache _cache = cache
            ?? throw new ArgumentNullException(nameof(cache));

        private readonly ICategoryRepository _categoryRepository = categoryRepository
            ?? throw new ArgumentNullException(nameof(categoryRepository));

        private readonly ICategoryTextRepository _categoryTextRepository = categoryTextRepository
            ?? throw new ArgumentNullException(nameof(categoryTextRepository));

        private readonly IConfiguration _config = config
            ?? throw new ArgumentNullException(nameof(config));

        private readonly IEmediaAccessRepository _emediaAccessRepository = emediaAccessRepository
            ?? throw new ArgumentNullException(nameof(emediaAccessRepository));

        private readonly IEmediaCategoryRepository _emediaCategoryRepository = emediaCategoryRepository
            ?? throw new ArgumentNullException(nameof(emediaCategoryRepository));

        private readonly IEmediaGroupRepository _emediaGroupRepository = emediaGroupRepository
            ?? throw new ArgumentNullException(nameof(emediaGroupRepository));

        private readonly IEmediaRepository _emediaRepository = emediaRepository
            ?? throw new ArgumentNullException(nameof(emediaRepository));

        private readonly IEmediaSubjectRepository _emediaSubjectRepository = emediaSubjectRepository
            ?? throw new ArgumentNullException(nameof(emediaSubjectRepository));

        private readonly IEmediaTextRepository _emediaTextRepository = emediaTextRepository
            ?? throw new ArgumentNullException(nameof(emediaTextRepository));

        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        private readonly LanguageService _languageService = languageService
            ?? throw new ArgumentNullException(nameof(languageService));

        private readonly SegmentService _segmentService = segmentService
            ?? throw new ArgumentNullException(nameof(segmentService));

        private int CachePagesInHours
        {
            get
            {
                return GetPageCacheDuration(_config);
            }
        }

        public async Task<Emedia> GetAsync(bool forceReload, string slug, bool countAccess)
        {
            var emedias = await GetEmediasAsync(forceReload);
            var emedia = emedias.SingleOrDefault(_ => _.Slug == slug);
            if (countAccess && emedia != null)
            {
                await _emediaAccessRepository.AddAccessLogAsync(emedia.Id);
            }
            return emedia;
        }

        public async Task<ICollection<Emedia>> GetEmediaAsync(bool forceReload)
        {
            var langIds
                = await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService);

            return await GetEmediasAsync(forceReload, langIds);
        }

        public async Task<ICollection<Emedia>> GetEmediaAsync(bool forceReload, EmediaFilter filter)
        {
            var langIds
                = await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService);

            var emedias = await GetEmediasAsync(forceReload, langIds);

            // perform filtering
            if (filter?.SubjectId.HasValue == true)
            {
                emedias = await FilterBySubjectAsync(emedias, filter.SubjectId.Value);
            }

            return emedias;
        }

        public async Task<ICollection<EmediaGroup>> GetGroupedEmediaAsync(bool forceReload)
        {
            var langIds
                = await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService);

            var emedias = await GetEmediasAsync(forceReload, langIds);

            // fetch groups and divide up into groups
            var groups = await GetEmediaGroupsAsync(forceReload);
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
            }

            return groups;
        }

        private async Task AddCategoriesAsync(ICollection<Emedia> emedias,
            bool forceReload,
            int[] langIds)
        {
            var categories = await GetCategoriesAsync(forceReload, langIds);

            var emediaCategories = await GetEmediaCategoriesAsync(forceReload);
            foreach (var emedia in emedias)
            {
                foreach (var languageId in langIds)
                {
                    emedia.EmediaText = await GetEmediaTextAsync(forceReload,
                        languageId,
                        emedia.Id);
                    if (emedia.EmediaText != null)
                    {
                        break;
                    }
                }

                var thisEmediaCategoryIds = emediaCategories
                    .Where(_ => _.EmediaId == emedia.Id)
                    .Select(_ => _.CategoryId);

                emedia.Categories
                    .AddRange([.. categories.Where(_ => thisEmediaCategoryIds.Contains(_.Id))]);
            }
        }

        private async Task<ICollection<Emedia>> FilterBySubjectAsync(ICollection<Emedia> emedias,
            int subjectId)
        {
            var subjectEmediaIds = await _emediaSubjectRepository.GetEmediaIdsAsync(subjectId);

            return [.. emedias.Where(_ => subjectEmediaIds.Contains(_.Id))];
        }

        private async Task<ICollection<Category>> GetCategoriesAsync(bool forceReload,
                    int[] langIds)
        {
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
                foreach (var languageId in langIds)
                {
                    category.CategoryText = await GetCategoryTextAsync(forceReload,
                        languageId,
                        category.Id);
                    if (category.CategoryText != null)
                    {
                        break;
                    }
                }
            }

            return categories;
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

        private async Task<ICollection<EmediaCategory>> GetEmediaCategoriesAsync(bool forceReload)
        {
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

            return emediaCategories;
        }

        private async Task<ICollection<EmediaGroup>> GetEmediaGroupsAsync(bool forceReload)
        {
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

            return groups;
        }

        /// <summary>
        /// Return <see cref="Emedia"/> objects from cache or the database if they are not cached.
        /// This call does not populate any category data and should not be used for display!
        /// </summary>
        /// <param name="forceReload">Ignore any cached value and force a database load</param>
        /// <returns><see cref="ICollection"/> of <see cref="Emedia"/> objects</returns>
        private async Task<ICollection<Emedia>> GetEmediasAsync(bool forceReload)
        {
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

            return emedias;
        }

        /// <summary>
        /// Return <see cref="Emedia"/> objects from cache or the database if they are not cached
        /// including categories for display.
        /// </summary>
        /// <param name="forceReload">Ignore any cached value and force a database load</param>
        /// <param name="langIds">Language ids in order of preference for fetching text</param>
        /// <returns><see cref="ICollection"/> of <see cref="Emedia"/> objects</returns>
        private async Task<ICollection<Emedia>> GetEmediasAsync(bool forceReload, int[] langIds)
        {
            ICollection<Emedia> emedias = await GetEmediasAsync(forceReload);

            // add category data
            await AddCategoriesAsync(emedias, forceReload, langIds);

            return emedias;
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