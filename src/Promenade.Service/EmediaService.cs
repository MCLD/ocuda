using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class EmediaService : BaseService<EmediaService>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryTextRepository _categoryTextRepository;
        private readonly IEmediaRepository _emediaRepository;
        private readonly IEmediaCategoryRepository _emediaCategoryRepository;
        private readonly IEmediaGroupRepository _emediaGroupRepository;
        private readonly IEmediaTextRepository _emediaTextRepository;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;
        private readonly LanguageService _languageService;

        public EmediaService(ILogger<EmediaService> logger,
            IDateTimeProvider dateTimeProvider,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository,
            ICategoryTextRepository categoryTextRepository,
            IEmediaRepository emediaRepository,
            IEmediaCategoryRepository emediaCategoryRepository,
            IEmediaGroupRepository emediaGroupRepository,
            IEmediaTextRepository emediaTextRepository,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _categoryTextRepository = categoryTextRepository
                ?? throw new ArgumentNullException(nameof(categoryTextRepository));
            _emediaRepository = emediaRepository
                ?? throw new ArgumentNullException(nameof(emediaRepository));
            _emediaCategoryRepository = emediaCategoryRepository
                ?? throw new ArgumentNullException(nameof(emediaCategoryRepository));
            _emediaGroupRepository = emediaGroupRepository
                ?? throw new ArgumentNullException(nameof(emediaGroupRepository));
            _emediaTextRepository = emediaTextRepository
                ?? throw new ArgumentNullException(nameof(emediaTextRepository));
            _segmentRepository = segmentRepository
                ?? throw new ArgumentNullException(nameof(segmentRepository));
            _segmentTextRepository = segmentTextRepository
                ?? throw new ArgumentNullException(nameof(segmentTextRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<ICollection<EmediaGroup>> GetGroupedEmediaAsync()
        {
            var currentCultureName = _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            int defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            int? currentLangaugeId = null;

            if (!string.IsNullOrWhiteSpace(currentCultureName))
            {
                currentLangaugeId = await _languageService.GetLanguageIdAsync(currentCultureName);
            }

            var groupedEmedia = await _emediaGroupRepository.GetAllWithEmediaAsync();

            foreach (var group in groupedEmedia)
            {
                group.Emedias = group.Emedias.OrderBy(_ => _.Name).ToList();

                if (group.SegmentId.HasValue)
                {
                    group.Segment = await _segmentRepository.GetActiveAsync(group.SegmentId.Value);

                    if (group.Segment != null)
                    {
                        if (currentLangaugeId.HasValue)
                        {
                            group.Segment.SegmentText = await _segmentTextRepository
                                .GetByIdsAsync(currentLangaugeId.Value, group.Segment.Id);
                        }
                        if (group.Segment.SegmentText == null)
                        {
                            group.Segment.SegmentText = await _segmentTextRepository
                                .GetByIdsAsync(defaultLanguageId, group.Segment.Id);
                        }
                    }
                }

                foreach (var emedia in group.Emedias)
                {
                    if (currentLangaugeId.HasValue)
                    {
                        emedia.EmediaText = await _emediaTextRepository.GetByIdsAsync(
                            emedia.Id, currentLangaugeId.Value);
                    }
                    if (emedia.EmediaText == null)
                    {
                        emedia.EmediaText = await _emediaTextRepository.GetByIdsAsync(
                            emedia.Id, defaultLanguageId);
                    }

                    emedia.Categories = await _emediaCategoryRepository
                        .GetCategoriesByEmediaIdAsync(emedia.Id);

                    foreach (var category in emedia.Categories)
                    {
                        if (currentLangaugeId.HasValue)
                        {
                            category.CategoryText = await _categoryTextRepository.GetByIdsAsync(
                                category.Id, currentLangaugeId.Value);
                        }
                        if (category.CategoryText == null)
                        {
                            category.CategoryText = await _categoryTextRepository.GetByIdsAsync(
                                category.Id, defaultLanguageId);
                        }
                    }
                }
            }

            return groupedEmedia;
        }
    }
}
