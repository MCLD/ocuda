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

        private readonly ILanguageRepository _languageRepository;
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;
        private readonly ISiteSettingPromService _siteSettingPromService;

        public NavigationService(ILogger<NavigationService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILanguageRepository languageRepository,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository,
            ISiteSettingPromService siteSettingPromService)
            : base(logger, httpContextAccessor)
        {
            _languageRepository = languageRepository
                ?? throw new ArgumentNullException(nameof(languageRepository));
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

        public async Task<RoleProperties> GetRolePropertiesForNavigationAsync(int id)
        {
            // denotes if a navigation is a parent, child or grandchild
            int depth = 0;

            var navigation = await _navigationRepository.FindAsync(id);

            while (navigation.NavigationId.HasValue)
            {
                // navigations past grandchild aren't allowed and should not be able to exist
                if (depth >= 2)
                {
                    _logger.LogCritical("Invalid navigation depth for navigation {id}", id);
                    throw new OcudaException("There is an invalid navigation setting, please contact a web administrator.");
                }

                navigation = await _navigationRepository.FindAsync(navigation.NavigationId.Value);
                depth++;
            }

            var role = await GetNavigationRoleAsync(navigation.Id);

            var roleProperties = new RoleProperties();

            if (depth != 0)
            {
                roleProperties.MustHaveText = true;
            }

            if ((depth == 1 && role != Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter)
                || depth == 0)
            {
                roleProperties.CanHaveChildren = true;

                if (depth == 0 && (role == Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop
                    || role == Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle))
                {
                    roleProperties.ChildrenCanChangeToLink = true;
                }

                if (depth == 0 && role == Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop)
                {
                    roleProperties.ChildrenCanHideText = true;
                }

                if (depth == 1 && role != Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop)
                {
                    roleProperties.ChildrenCanTargetNewWindow = true;
                }

                if ((depth == 0 && role != Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft)
                    || depth == 1)
                {
                    roleProperties.ChildrenCanDisplayIcon = true;
                }
            }

            if (depth == 0 && role != Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter)
            {
                roleProperties.CanHaveGrandchildren = true;
            }

            return roleProperties;
        }

        public async Task<Navigation> GetByIdAsync(int id)
        {
            return await _navigationRepository.FindAsync(id);
        }

        public async Task<ICollection<Navigation>> GetNavigationChildrenAsync(int id)
        {
            return await _navigationRepository.GetChildrenAsync(id);
        }

        public async Task<ICollection<string>> GetNavigationLanguagesByIdAsync(int id)
        {
            return await _navigationTextRepository.GetUsedLanguageNamesByNavigationId(id);
        }

        public async Task<ICollection<Navigation>> GetTopLevelNavigationsAsync()
        {
            return await _navigationRepository.GetTopLevelNavigationsAsync();
        }

        public async Task<int> GetSubnavigationCountAsnyc(int id)
        {
            return await _navigationRepository.GetSubnavigationCountAsync(id);
        }

        public async Task<NavigationText> GetTextByNavigationAndLanguageAsync(int navigationId,
            int languageId)
        {
            return await _navigationTextRepository.GetByNavigationAndLanguageAsync(navigationId,
                languageId);
        }

        public async Task<Navigation> CreateAsync(Navigation navigation, string siteSetting = null)
        {
            int? maxSortOrder = null;
            RoleProperties parentRoleProperties = null;

            if (navigation.NavigationId.HasValue)
            {
                parentRoleProperties = await GetRolePropertiesForNavigationAsync(
                    navigation.NavigationId.Value);

                maxSortOrder = await _navigationRepository.GetMaxSortOrderAsync(
                    navigation.NavigationId.Value);
            }

            navigation.Name = navigation.Name?.Trim();

            if (parentRoleProperties != null)
            {
                if (string.IsNullOrWhiteSpace(navigation.NavigationText?.Label)
                    && string.IsNullOrWhiteSpace(navigation.NavigationText?.Link)
                    && string.IsNullOrWhiteSpace(navigation.NavigationText?.Title))
                {
                    throw new OcudaException("At least one text field must be filled.");
                }

                var navigationText = new NavigationText();

                navigationText.Label = navigation.NavigationText.Label?.Trim();
                navigationText.Link = navigation.NavigationText.Link?.Trim();
                navigationText.Title = navigation.NavigationText.Title?.Trim();

                navigationText.LanguageId = await _languageRepository.GetDefaultLanguageId();
                navigationText.Navigation = navigation;

                await _navigationTextRepository.AddAsync(navigationText);
            }

            if (parentRoleProperties?.ChildrenCanDisplayIcon == true)
            {
                navigation.Icon = navigation.Icon?.Trim();
            }
            else
            {
                navigation.Icon = null;
            }

            if (parentRoleProperties?.ChildrenCanChangeToLink != true)
            {
                navigation.ChangeToLinkWhenExtraSmall = false;
            }
            if (parentRoleProperties?.ChildrenCanHideText != true)
            {
                navigation.HideTextWhenExtraSmall = false;
            }
            if (parentRoleProperties?.ChildrenCanTargetNewWindow != true)
            {
                navigation.TargetNewWindow = false;
            }

            if (maxSortOrder.HasValue)
            {
                navigation.Order = maxSortOrder.Value + 1;
            }
            else
            {
                navigation.Order = 0;
            }

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

            RoleProperties parentRoleProperties = null;

            if (currentNavigation.NavigationId.HasValue)
            {
                parentRoleProperties = await GetRolePropertiesForNavigationAsync(
                    navigation.NavigationId.Value);
            }

            currentNavigation.Name = navigation.Name?.Trim();

            if (parentRoleProperties?.ChildrenCanDisplayIcon == true)
            {
                currentNavigation.Icon = navigation.Icon?.Trim();
            }
            if (parentRoleProperties?.ChildrenCanChangeToLink == true)
            {
                currentNavigation.ChangeToLinkWhenExtraSmall = navigation.ChangeToLinkWhenExtraSmall;
            }
            if (parentRoleProperties?.ChildrenCanHideText == true)
            {
                currentNavigation.HideTextWhenExtraSmall = navigation.HideTextWhenExtraSmall;
            }
            if (parentRoleProperties?.ChildrenCanTargetNewWindow == true)
            {
                currentNavigation.TargetNewWindow = navigation.TargetNewWindow;
            }

            _navigationRepository.Update(currentNavigation);
            await _navigationRepository.SaveAsync();
            return currentNavigation;
        }

        public async Task DeleteAsync(int id)
        {
            var navigation = await _navigationRepository.FindAsync(id);

            if (navigation == null)
            {
                throw new OcudaException("Navigation does not exist.");
            }

            (var navigations, var navigationIds) = await GetFlattenedNavigationsAsync(navigation);

            var navigationTexts = await _navigationTextRepository
                .GetByNavigationIdsAsync(navigationIds);

            if (!navigation.NavigationId.HasValue)
            {
                var role = await GetNavigationRoleAsync(navigation.Id);

                if (!string.IsNullOrWhiteSpace(role))
                {
                    await _siteSettingPromService.UpdateAsync(role, DefaultNavigationSiteSettingId);
                }
            }
            else
            {
                var subsequentNavigations = await _navigationRepository
                    .GetSubsequentNavigationsAsync(navigation.Order, navigation.NavigationId.Value);

                if (subsequentNavigations.Count > 0)
                {
                    subsequentNavigations.ForEach(_ => _.Order--);
                    _navigationRepository.UpdateRange(subsequentNavigations);
                }
            }

            _navigationTextRepository.RemoveRange(navigationTexts);
            _navigationRepository.RemoveRange(navigations);

            await _navigationRepository.SaveAsync();
        }

        public async Task UpdateSortOrder(int id, bool increase)
        {
            var navigation = await _navigationRepository.FindAsync(id);

            if (!navigation.NavigationId.HasValue)
            {
                throw new OcudaException("Top level navigations cannot be sorted.");
            }

            int newSortOrder;
            if (increase)
            {
                newSortOrder = navigation.Order + 1;
            }
            else
            {
                if (navigation.Order == 0)
                {
                    throw new OcudaException("Navigation is already in the first position.");
                }
                newSortOrder = navigation.Order - 1;
            }

            var navigationInPosition = await _navigationRepository.GetByOrderAndParentAsync(
                newSortOrder,
                navigation.NavigationId.Value);

            if (navigationInPosition == null)
            {
                throw new OcudaException("Navigation is already in the last position.");
            }

            navigationInPosition.Order = navigation.Order;
            navigation.Order = newSortOrder;

            _navigationRepository.Update(navigation);
            _navigationRepository.Update(navigationInPosition);
            await _navigationRepository.SaveAsync();
        }

        public async Task SetNavigationTextAsync(NavigationText navigationText)
        {
            var roleProperties = await GetRolePropertiesForNavigationAsync(
                navigationText.NavigationId);

            if (!roleProperties.MustHaveText)
            {
                throw new OcudaException("Navigation cannot have text.");
            }

            if (string.IsNullOrWhiteSpace(navigationText.Label)
                && string.IsNullOrWhiteSpace(navigationText.Link)
                && string.IsNullOrWhiteSpace(navigationText.Title))
            {
                throw new OcudaException("At least one text field must be filled.");
            }

            var currentText = await _navigationTextRepository.GetByNavigationAndLanguageAsync(
                navigationText.NavigationId, navigationText.LanguageId);

            if (currentText == null)
            {
                navigationText.Label = navigationText.Label?.Trim();
                navigationText.Link = navigationText.Link?.Trim();
                navigationText.Title = navigationText.Title?.Trim();

                await _navigationTextRepository.AddAsync(navigationText);
            }
            else
            {
                currentText.Label = navigationText.Label?.Trim();
                currentText.Link = navigationText.Link?.Trim();
                currentText.Title = navigationText.Title?.Trim();

                _navigationTextRepository.Update(currentText);
            }

            await _navigationTextRepository.SaveAsync();
        }

        public async Task DeleteNavigationTextAsync(int navigationId, int languageId)
        {
            var defaultLanguageId = await _languageRepository.GetDefaultLanguageId();

            if (languageId == defaultLanguageId)
            {
                throw new OcudaException("Cannot delete text for the default language");
            }

            var navigationText = await _navigationTextRepository.GetByNavigationAndLanguageAsync(
                navigationId,
                languageId);

            _navigationTextRepository.Remove(navigationText);
            await _navigationTextRepository.SaveAsync();
        }

        private async Task<(List<Navigation>, List<int>)> GetFlattenedNavigationsAsync(
            Navigation navigation)
        {
            var navigations = new List<Navigation>() { navigation };
            var navigationIds = new List<int>() { navigation.Id };

            var children = await _navigationRepository.GetChildrenAsync(navigation.Id);
            foreach (var child in children)
            {
                (var childNavigations, var childIds) = await GetFlattenedNavigationsAsync(child);

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

            return null;
        }
    }
}
