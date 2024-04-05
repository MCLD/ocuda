using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly ILanguageService _languageService;
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;
        private readonly ISiteSettingPromService _siteSettingPromService;
        private readonly string DefaultNavigationSiteSettingId = "-1";

        private IDictionary<int, string> _activeLanguageNames;

        public NavigationService(ILogger<NavigationService> logger,
                    IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository,
            ISiteSettingPromService siteSettingPromService)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navigationRepository);
            ArgumentNullException.ThrowIfNull(navigationTextRepository);
            ArgumentNullException.ThrowIfNull(siteSettingPromService);

            _languageService = languageService;
            _navigationRepository = navigationRepository;
            _navigationTextRepository = navigationTextRepository;
            _siteSettingPromService = siteSettingPromService;
        }

        public async Task<Navigation> CreateAsync(Navigation navigation, string siteSetting = null)
        {
            if (navigation == null)
            {
                throw new ArgumentNullException(nameof(navigation));
            }

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

                await _navigationTextRepository.AddAsync(new NavigationText
                {
                    Label = navigation.NavigationText.Label?.Trim(),
                    Link = navigation.NavigationText.Link?.Trim(),
                    Title = navigation.NavigationText.Title?.Trim(),

                    LanguageId = await _languageService.GetDefaultLanguageId(),
                    Navigation = navigation
                });
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
                await _siteSettingPromService.UpdateAsync(siteSetting,
                    navigation.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return navigation;
        }

        public async Task DeleteAsync(int id)
        {
            var navigation = await _navigationRepository.FindAsync(id)
                ?? throw new OcudaException("Navigation does not exist.");

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

        public async Task DeleteNavigationTextAsync(int navigationId, int languageId)
        {
            var defaultLanguageId = await _languageService.GetDefaultLanguageId();

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

        public async Task<Navigation> EditAsync(Navigation navigation)
        {
            if (navigation == null)
            {
                throw new ArgumentNullException(nameof(navigation));
            }

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

        public async Task<IEnumerable<Navigation>> GetNavigationTreeAsync(int navigationId)
        {
            var children = await GetNavigationChildrenAsync(navigationId);
            await AddNavigationTextsAsync(children);
            foreach (var child in children)
            {
                child.Navigations = await GetNavigationTreeAsync(child.Id);
            }
            return children;
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

        public async Task<int> GetSubnavigationCountAsync(int id)
        {
            return await _navigationRepository.GetSubnavigationCountAsync(id);
        }

        public async Task<NavigationText> GetTextByNavigationAndLanguageAsync(int navigationId,
            int languageId)
        {
            return await _navigationTextRepository.GetByNavigationAndLanguageAsync(navigationId,
                languageId);
        }

        public async Task<ICollection<Navigation>> GetTopLevelNavigationsAsync()
        {
            return await _navigationRepository.GetTopLevelNavigationsAsync();
        }

        public async Task ReplaceAllNavigationAsync(IEnumerable<Navigation> navigations)
        {
            ArgumentNullException.ThrowIfNull(navigations);

            // delete existing navigations, navigation texts
            await _navigationTextRepository.RemoveAllAsync();
            await _navigationRepository.RemoveAllAsync();

            // walk navigations and insert new ones
            foreach (var navigation in navigations)
            {
                await InsertNavigationAsync(navigation, null);
            }
        }

        public async Task SetNavigationTextAsync(NavigationText navigationText)
        {
            if (navigationText == null)
            {
                throw new ArgumentNullException(nameof(navigationText));
            }

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
                navigation.NavigationId.Value)
                ?? throw new OcudaException("Navigation is already in the last position.");
            navigationInPosition.Order = navigation.Order;
            navigation.Order = newSortOrder;

            _navigationRepository.Update(navigation);
            _navigationRepository.Update(navigationInPosition);
            await _navigationRepository.SaveAsync();
        }

        private async Task AddNavigationTextsAsync(IEnumerable<Navigation> navigations)
        {
            if (navigations == null)
            {
                return;
            }

            var texts = await _navigationTextRepository.
                GetByNavigationIdsAsync(navigations.Select(_ => _.Id).ToList());

            foreach (var navigation in navigations)
            {
                navigation.AllNavigationTexts = texts.Where(_ => _.NavigationId == navigation.Id);
                foreach (var navigationText in navigation.AllNavigationTexts)
                {
                    var activeLanguageName = await
                        GetActiveLanguageNameAsync(navigationText.LanguageId);

                    if (!string.IsNullOrEmpty(activeLanguageName))
                    {
                        navigationText.Language = new Language
                        {
                            Name = activeLanguageName
                        };
                    }
                }
            }
        }

        private async Task<int?> GetActiveLanguageIdAsync(string name)
        {
            _activeLanguageNames ??= await _languageService.GetActiveNamesAsync();
            return _activeLanguageNames
                .Where(_ => _.Value == name)
                .Select(_ => _.Key)
                .SingleOrDefault();
        }

        private async Task<string> GetActiveLanguageNameAsync(int id)
        {
            _activeLanguageNames ??= await _languageService.GetActiveNamesAsync();
            return _activeLanguageNames.TryGetValue(id, out string value)
                ? value
                : null;
        }

        private async Task<(ICollection<Navigation>, ICollection<int>)> GetFlattenedNavigationsAsync(
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

        private async Task InsertNavigationAsync(Navigation navigation, int? parentId)
        {
            navigation.Id = default;
            navigation.NavigationId = null;
            var children = navigation.Navigations;
            navigation.Navigations = null;

            if (parentId.HasValue)
            {
                navigation.NavigationId = parentId.Value;
            }

            await _navigationRepository.AddAsync(navigation);
            await _navigationRepository.SaveAsync();

            // update system setting if necessary
            if (!string.IsNullOrEmpty(navigation.NavigationRole))
            {
                await UpdateNavigationRoleAsync(navigation.Id, navigation.NavigationRole);
            }

            // handle text
            if (navigation.AllNavigationTexts != null)
            {
                foreach (var navigationText in navigation.AllNavigationTexts)
                {
                    var requestedLanguageName = navigationText.Language?.Name;
                    navigationText.NavigationId = navigation.Id;
                    navigationText.Language = null;
                    navigationText.LanguageId = 0;
                    if (!string.IsNullOrEmpty(requestedLanguageName))
                    {
                        var languageLookup
                            = await GetActiveLanguageIdAsync(requestedLanguageName);
                        if (languageLookup.HasValue)
                        {
                            navigationText.LanguageId = languageLookup.Value;
                            await _navigationTextRepository.AddAsync(navigationText);
                        }
                    }
                }
            }
            await _navigationTextRepository.SaveAsync();

            // handle children
            foreach (var childNavigation in children)
            {
                await InsertNavigationAsync(childNavigation, navigation.Id);
            }
        }

        private async Task UpdateNavigationRoleAsync(int navigationId, string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return;
            }

            string setting = null;

            if (role == nameof(NavigationRoles.Top))
            {
                setting = Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop;
            }
            else if (role == nameof(NavigationRoles.Left))
            {
                setting = Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft;
            }
            else if (role == nameof(NavigationRoles.Middle))
            {
                setting = Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle;
            }
            else if (role == nameof(NavigationRoles.Footer))
            {
                setting = Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter;
            }

            if (!string.IsNullOrEmpty(setting))
            {
                await _siteSettingPromService.UpdateAsync(setting,
                    navigationId.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}