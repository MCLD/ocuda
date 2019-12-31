using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class NavigationService : BaseService<NavigationService>
    {
        private readonly LanguageService _languageService;
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;

        public NavigationService(ILogger<NavigationService> logger,
            IDateTimeProvider dateTimeProvider,
            LanguageService languageService,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository) : base(logger, dateTimeProvider)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _navigationRepository = navigationRepository
                ?? throw new ArgumentNullException(nameof(navigationRepository));
            _navigationTextRepository = navigationTextRepository
                ?? throw new ArgumentNullException(nameof(navigationTextRepository));
        }

        public async Task<Navigation> GetNavigation(int navigationId)
        {
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            var nav = await _navigationRepository.FindAsync(navigationId);
            if (nav.NavigationTextId != null)
            {
                nav.NavigationText = await _navigationTextRepository
                    .FindAsync((int)nav.NavigationTextId, defaultLanguageId);
            }
            nav.Navigations = await GetNavigationChildren(navigationId, defaultLanguageId);
            return nav;
        }

        private async Task<ICollection<Navigation>> GetNavigationChildren(int navigationId,
            int languageId)
        {
            var children = await _navigationRepository.GetChildren(navigationId);
            foreach (var child in children)
            {
                child.Navigations = await GetNavigationChildren(child.Id, languageId);
                if (child.NavigationTextId != null)
                {
                    child.NavigationText = await _navigationTextRepository
                        .FindAsync((int)child.NavigationTextId, languageId);
                }
            }
            return children;
        }
    }
}
