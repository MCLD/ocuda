using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class NavigationService : BaseService<NavigationService>, INavigationService
    {
        private readonly string DefaultNavigationSiteSettingId = "-1";

        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;
        private readonly ISiteSettingPromService _siteSettingPromService;

        public NavigationService(ILogger<NavigationService> logger,
            IHttpContextAccessor httpContextAccessor,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository,
            ISiteSettingPromService siteSettingPromService)
            : base(logger, httpContextAccessor)
        {
            _navigationRepository = navigationRepository
                ?? throw new ArgumentNullException(nameof(navigationRepository));
            _navigationTextRepository = navigationTextRepository
                ?? throw new ArgumentNullException(nameof(navigationTextRepository));
            _siteSettingPromService = siteSettingPromService
                ?? throw new ArgumentNullException(nameof(siteSettingPromService));
        }

        public async Task<NavigationRoles> GetNavigationRolesAsync()
        {
            var navigationRoles = new NavigationRoles
            {
                Top = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop),
                Middle = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle),
                Left = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft),
                Footer = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter)
            };

            if (navigationRoles.Top < 0)
            {
                navigationRoles.Top = null;
            }
            if (navigationRoles.Middle < 0)
            {
                navigationRoles.Middle = null;
            }
            if (navigationRoles.Left < 0)
            {
                navigationRoles.Left = null;
            }
            if (navigationRoles.Footer < 0)
            {
                navigationRoles.Footer = null;
            }

            return navigationRoles;
        }

        public async Task<Navigation> GetByIdAsync(int id)
        {
            return await _navigationRepository.FindAsync(id);
        }

        public async Task<ICollection<Navigation>> GetNavigationChildrenAsync(int id)
        {
            return await _navigationRepository.GetChildrenAsync(id);
        }

        public async Task<ICollection<Navigation>> GetTopLevelNavigationsAsync()
        {
            return await _navigationRepository.GetTopLevelNavigationsAsync();
        }

        public async Task<Navigation> CreateAsync(Navigation navigation, string siteSetting = null)
        {
            navigation.Icon = navigation.Icon?.Trim();
            navigation.Name = navigation.Name?.Trim();

            await _navigationRepository.AddAsync(navigation);
            await _navigationRepository.SaveAsync();

            if (!string.IsNullOrWhiteSpace(siteSetting))
            {
                await _siteSettingPromService.UpdateAsync(siteSetting, navigation.Id.ToString());
            }

            return navigation;
        }

        public async Task<Navigation> EditAsync(Navigation navigation)
        {
            var currentNavigation = await _navigationRepository.FindAsync(navigation.Id);

            currentNavigation.ChangeToLinkWhenExtraSmall = navigation.ChangeToLinkWhenExtraSmall;
            currentNavigation.HideTextWhenExtraSmall = navigation.HideTextWhenExtraSmall;
            currentNavigation.Icon = navigation.Icon?.Trim();
            currentNavigation.Name = navigation.Name?.Trim();
            currentNavigation.TargetNewWindow = navigation.TargetNewWindow;

            _navigationRepository.Update(navigation);
            await _navigationRepository.SaveAsync();
            return navigation;
        }

        public async Task DeleteAsync(int id)
        {
            var navigation = await _navigationRepository.FindAsync(id);

            if (navigation == null)
            {
                throw new OcudaException("Navigation does not exist.");
            }

            (var navigations, var navigationIds) = await GetFlattenedNavigationAsync(navigation);

            var navigationTexts = await _navigationTextRepository
                .GetByNavigationIdsAsync(navigationIds);

            if (!navigation.NavigationId.HasValue)
            {
                var role = await GetNavigationRoleAsync(navigation.Id);
                await _siteSettingPromService.UpdateAsync(role, DefaultNavigationSiteSettingId);
            }

            _navigationTextRepository.RemoveRange(navigationTexts);
            _navigationRepository.RemoveRange(navigations);

            await _navigationRepository.SaveAsync();
        }

        private async Task<(List<Navigation>, List<int>)> GetFlattenedNavigationAsync(
            Navigation navigation)
        {
            var navigations = new List<Navigation>() { navigation };
            var navigationIds = new List<int>() { navigation.Id };

            var children = await _navigationRepository.GetChildrenAsync(navigation.Id);
            foreach (var child in children)
            {
                (var childNavigations, var childIds) = await GetFlattenedNavigationAsync(child);

                navigations.AddRange(childNavigations);
                navigationIds.AddRange(childIds);
            }

            return (navigations, navigationIds);
        }

        private async Task<string> GetNavigationRoleAsync(int id)
        {
            if (id == await _siteSettingPromService
                   .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop))
            {
                return Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop;
            }
            else if (id == await _siteSettingPromService
                   .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle))
            {
                return Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle;
            }
            else if (id == await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft))
            {
                return Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft;
            }
            else if (id == await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter))
            {
                return Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter;
            }

            _logger.LogError("Navigation {id} does not have a role and cannot be deleted.", id);
            throw new OcudaException("Primary navigation does not belong to a role.");
        }
    }
}
