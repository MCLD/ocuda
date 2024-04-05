using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class NavigationsController : BaseController<NavigationsController>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILanguageService _languageService;
        private readonly INavigationService _navigationService;
        private readonly IPermissionGroupService _permissionGroupService;

        public NavigationsController(ServiceFacades.Controller<NavigationsController> context,
            IDateTimeProvider dateTimeProvider,
            ILanguageService languageService,
            INavigationService navigationService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navigationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _dateTimeProvider = dateTimeProvider;
            _languageService = languageService;
            _navigationService = navigationService;
            _permissionGroupService = permissionGroupService;
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Navigations"; } }

        [HttpPost("[action]")]
        public async Task<JsonResult> ChangeSort(int id, bool increase)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(_permissionGroupService,
               ApplicationPermission.NavigationManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else
            {
                try
                {
                    await _navigationService.UpdateSortOrder(id, increase);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateNavigation(Navigation navigation, string role)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    navigation = await _navigationService.CreateAsync(navigation, role);

                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Details), new { id = navigation.Id })
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteNavigation(Navigation navigation)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            if (navigation == null)
            {
                ShowAlertDanger("Unable to create navigation: navigation is null.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _navigationService.DeleteAsync(navigation.Id);
                ShowAlertSuccess($"Deleted navigation: {navigation.Name}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to delete navigation: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting navigation: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting navigation: {navigation.Name}");
            }

            if (navigation.NavigationId.HasValue)
            {
                return RedirectToAction(nameof(Details),
                    new { id = navigation.NavigationId.Value });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteText(DetailsViewModel model)
        {
            if (model == null)
            {
                ShowAlertDanger("Unable to delete empty navigation.");
                return RedirectToAction(nameof(Index));
            }

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService
                .GetActiveByIdAsync(model.NavigationText.LanguageId);

            try
            {
                await _navigationService.DeleteNavigationTextAsync(
                    model.NavigationText.NavigationId,
                    model.NavigationText.LanguageId);

                ShowAlertSuccess($"Deleted {language.Description} navigation text");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to delete navigation text: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting navigation text: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting {language.Description} navigation text");
            }

            return RedirectToAction(nameof(Details), new
            {
                id = model.NavigationText.NavigationId,
                language = language.Name
            });
        }

        [HttpGet("{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id, string language)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            var navigation = await _navigationService.GetByIdAsync(id);

            if (navigation == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DetailsViewModel
            {
                Navigation = navigation,
                RoleProperties = await _navigationService.GetRolePropertiesForNavigationAsync(id)
            };

            if (viewModel.RoleProperties.MustHaveText)
            {
                var languages = await _languageService.GetActiveAsync();

                var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

                viewModel.LanguageDescription = selectedLanguage.Description;
                viewModel.LanguageId = selectedLanguage.Id;

                viewModel.LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name);

                viewModel.NavigationText = await _navigationService
                    .GetTextByNavigationAndLanguageAsync(id, selectedLanguage.Id);

                viewModel.CanDeleteText = viewModel.NavigationText != null
                    && !selectedLanguage.IsDefault;
            }

            if (viewModel.RoleProperties.CanHaveChildren)
            {
                var childNavigations = await _navigationService
                    .GetNavigationChildrenAsync(id);

                foreach (var childNavigation in childNavigations)
                {
                    childNavigation.NavigationLanguages.AddRange(await _navigationService
                        .GetNavigationLanguagesByIdAsync(childNavigation.Id));

                    if (viewModel.RoleProperties.CanHaveGrandchildren)
                    {
                        childNavigation.SubnavigationCount = await _navigationService
                            .GetSubnavigationCountAsync(childNavigation.Id);
                    }
                }

                viewModel.Navigations = childNavigations;
            }

            return View(viewModel);
        }

        [HttpPost("{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(DetailsViewModel model)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            if (model == null)
            {
                ShowAlertDanger("Unable to save empty navigation.");
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _navigationService.SetNavigationTextAsync(model.NavigationText);
                    ShowAlertSuccess("Updated navigation text");
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to update navigation text: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating navigation text: {Message}", ex.Message);
                    ShowAlertDanger("Error updating navigation text");
                }
            }

            var language = await _languageService.GetActiveByIdAsync(
                model.NavigationText.LanguageId);

            return RedirectToAction(nameof(Details), new
            {
                id = model.NavigationText.NavigationId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EditNavigation(Navigation navigation)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    await _navigationService.EditAsync(navigation);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Export()
        {
            var baseNavigationTree = await _navigationService.GetTopLevelNavigationsAsync();
            var navigationRoles = await _navigationService.GetNavigationRolesAsync();
            var navigationTree = baseNavigationTree
                .OrderByDescending(_ => _.Id == navigationRoles.Top)
                .ThenByDescending(_ => _.Id == navigationRoles.Middle)
                .ThenByDescending(_ => _.Id == navigationRoles.Left)
                .ThenByDescending(_ => _.Id == navigationRoles.Footer)
                .ToList();

            foreach (var navigation in navigationTree)
            {
                navigation.Navigations = await _navigationService
                    .GetNavigationTreeAsync(navigation.Id);
                navigation.NavigationRole = GetNavigationRole(navigationRoles, navigation.Id);
            }

            string publicSitePath = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

            string intranetPath = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.UserInterface.BaseIntranetLink);

            return File(JsonSerializer.SerializeToUtf8Bytes(new PortableList<Navigation>
            {
                ExportedAt = _dateTimeProvider.Now,
                ExportedBy = CurrentUsername,
                Items = navigationTree,
                Source = $"{publicSitePath} (via {intranetPath})",
                Type = nameof(Navigation),
                Version = 1
            }),
                System.Net.Mime.MediaTypeNames.Application.Json,
                $"{_dateTimeProvider.Now:yyyyMMdd}-{nameof(Navigation)}.json");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Import(IFormFile navigationJson)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            if (navigationJson == null)
            {
                return StatusCode(500);
            }

            var jsonStream = navigationJson.OpenReadStream();
            var navigationsFromFile = await JsonSerializer
                .DeserializeAsync<PortableList<Navigation>>(jsonStream);

            await _navigationService.ReplaceAllNavigationAsync(navigationsFromFile.Items);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var hasEditPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement);

            if (!hasEditPermission)
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new IndexViewModel
            {
                HasEditPermission = hasEditPermission,
                NavigationRoles = await _navigationService.GetNavigationRolesAsync()
            };

            var topLevelNavigations = await _navigationService.GetTopLevelNavigationsAsync();
            viewModel.Navigations.AddRange(topLevelNavigations
                .OrderByDescending(_ => _.Id == viewModel.NavigationRoles.Top)
                .ThenByDescending(_ => _.Id == viewModel.NavigationRoles.Middle)
                .ThenByDescending(_ => _.Id == viewModel.NavigationRoles.Left)
                .ThenByDescending(_ => _.Id == viewModel.NavigationRoles.Footer)
                .ToList());

            foreach (var navigation in viewModel.Navigations)
            {
                navigation.SubnavigationCount = await _navigationService
                    .GetSubnavigationCountAsync(navigation.Id);
            }

            var openRoles = new List<SelectListItem>();
            if (!viewModel.NavigationRoles.Top.HasValue)
            {
                openRoles.Add(new SelectListItem(nameof(NavigationRoles.Top),
                    Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop));
            }
            if (!viewModel.NavigationRoles.Middle.HasValue)
            {
                openRoles.Add(new SelectListItem(nameof(NavigationRoles.Middle),
                    Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle));
            }
            if (!viewModel.NavigationRoles.Left.HasValue)
            {
                openRoles.Add(new SelectListItem(nameof(NavigationRoles.Left),
                    Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft));
            }
            if (!viewModel.NavigationRoles.Footer.HasValue)
            {
                openRoles.Add(new SelectListItem(nameof(NavigationRoles.Footer),
                    Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter));
            }

            viewModel.OpenRoles.AddRange(openRoles);

            return View(viewModel);
        }

        private static string GetNavigationRole(NavigationRoles roles, int? navigationId)
        {
            if (roles.Top == navigationId)
            {
                return nameof(roles.Top);
            }
            else if (roles.Middle == navigationId)
            {
                return nameof(roles.Middle);
            }
            else if (roles.Left == navigationId)
            {
                return nameof(roles.Left);
            }
            else if (roles.Footer == navigationId)
            {
                return nameof(roles.Footer);
            }
            else
            {
                return null;
            }
        }
    }
}