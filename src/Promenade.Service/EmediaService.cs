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
        private readonly IEmediaTextRepository _emediaTextRepository;
        private readonly LanguageService _languageService;

        public EmediaService(ILogger<EmediaService> logger,
            IDateTimeProvider dateTimeProvider,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository,
            ICategoryTextRepository categoryTextRepository,
            IEmediaRepository emediaRepository,
            IEmediaCategoryRepository emediaCategoryRepository,
            IEmediaTextRepository emediaTextRepository,
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
            _emediaTextRepository = emediaTextRepository
                ?? throw new ArgumentNullException(nameof(emediaTextRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<List<Emedia>> GetAllEmediaAsync()
        {
            var currentCultureName = _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            int? currentLangaugeId = null;
            int? defaultLanguageId = null;

            var emedias = await _emediaRepository.GetAllAsync();

            foreach (var emedia in emedias)
            {
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    if (!currentLangaugeId.HasValue)
                    {
                        currentLangaugeId = await _languageService
                            .GetLanguageIdAsync(currentCultureName);
                    }
                    emedia.EmediaText = await _emediaTextRepository.GetByIdsAsync(
                        emedia.Id, currentLangaugeId.Value);
                }
                if (emedia.EmediaText == null)
                {
                    if (!defaultLanguageId.HasValue)
                    {
                        defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                    }
                    emedia.EmediaText = await _emediaTextRepository.GetByIdsAsync(
                        emedia.Id, defaultLanguageId.Value);
                }

                emedia.Categories = await _emediaCategoryRepository
                    .GetCategoriesByEmediaIdAsync(emedia.Id);

                foreach (var category in emedia.Categories)
                {
                    if (!string.IsNullOrWhiteSpace(currentCultureName))
                    {
                        if (!currentLangaugeId.HasValue)
                        {
                            currentLangaugeId = await _languageService
                                .GetLanguageIdAsync(currentCultureName);
                        }
                        category.CategoryText = await _categoryTextRepository.GetByIdsAsync(
                            category.Id, currentLangaugeId.Value);
                    }
                    if (category.CategoryText == null)
                    {
                        if (!defaultLanguageId.HasValue)
                        {
                            defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                        }
                        category.CategoryText = await _categoryTextRepository.GetByIdsAsync(
                            category.Id, defaultLanguageId.Value);
                    }
                }
            }

            return emedias;
        }
    }
}
