using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class NavigationService : BaseService<NavigationService>
    {
        private readonly IOcudaCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;

        public NavigationService(ILogger<NavigationService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IHttpContextAccessor httpContextAccessor,
            LanguageService languageService,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _navigationRepository = navigationRepository
                ?? throw new ArgumentNullException(nameof(navigationRepository));
            _navigationTextRepository = navigationTextRepository
                ?? throw new ArgumentNullException(nameof(navigationTextRepository));
        }

        public async Task<Navigation> GetNavigation(int navigationId)
        {
            return await GetNavigation(navigationId, false);
        }

        public async Task<Navigation> GetNavigation(int navigationId, bool forceReload)
        {
            var currentCultureName = _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            var currentLanguageId = await _languageService.GetLanguageIdAsync(currentCultureName);
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync(forceReload);

            Navigation nav = null;

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavLang,
                navigationId,
                currentLanguageId);

            if (!forceReload)
            {
                nav = await _cache.GetObjectFromCacheAsync<Navigation>(cacheKey);
            }

            if (nav == null)
            {
                nav = await _navigationRepository.FindAsync(navigationId);

                nav.Navigations = await GetNavigationChildren(navigationId,
                    currentLanguageId,
                    defaultLanguageId);

                nav.NavigationText = await GetNavigationTextAsync(nav.Id,
                    currentLanguageId,
                    defaultLanguageId);

                await _cache.SaveToCacheAsync(cacheKey, nav, null, CacheSlidingExpiration);
            }

            return nav;
        }

        private async Task<ICollection<Navigation>> GetNavigationChildren(int navigationId,
            int languageId,
            int defaultLanguageId)
        {
            var children = await _navigationRepository.GetChildren(navigationId);
            foreach (var child in children)
            {
                child.Navigations = await GetNavigationChildren(child.Id,
                    languageId,
                    defaultLanguageId);

                child.NavigationText = await GetNavigationTextAsync(child.Id,
                    languageId,
                    defaultLanguageId);
            }
            return children;
        }

        private async Task<NavigationText> GetNavigationTextAsync(int navigationId,
            int languageId,
            int defaultLanguageId)
        {
            var navigationText = await _navigationTextRepository.GetByIdsAsync(
                    navigationId,
                    languageId);

            if (navigationText == null)
            {
                navigationText = await _navigationTextRepository.GetByIdsAsync(
                    navigationId,
                    defaultLanguageId);
            }

            return navigationText;
        }
    }
}