using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Promenade.Service
{
    public class PageService : BaseService<PageService>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPageRepository _pageRepository;
        private readonly LanguageService _languageService;

        public PageService(ILogger<PageService> logger,
            IDateTimeProvider dateTimeProvider,
            IHttpContextAccessor httpContextAccessor,
            IPageRepository pageRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<Page> GetByStubAndType(string stub, PageType type)
        {
            var formattedStub = stub?.Trim();

            var currentCultureName = _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            Page page = null;

            if (!string.IsNullOrWhiteSpace(currentCultureName))
            {
                var currentLangaugeId = await _languageService
                    .GetLanguageIdAsync(currentCultureName);
                page = await _pageRepository.GetPublishedByStubAndTypeAsync(formattedStub,
                    type, currentLangaugeId);

            }

            if (page == null)
            {
                var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                page = await _pageRepository.GetPublishedByStubAndTypeAsync(formattedStub,
                    type, defaultLanguageId);
            }

            return page;
        }
    }
}
