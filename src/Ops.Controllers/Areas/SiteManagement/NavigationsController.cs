using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class NavigationsController : BaseController<NavigationsController>
    {
        private readonly ILanguageService _languageService;
        private readonly INavigationService _navigationService;
        private readonly IPermissionGroupService _permissionGroupService;

        public static string Name { get { return "Navigations"; } }
        public static string Area { get { return "SiteManagement"; } }

        public NavigationsController(ServiceFacades.Controller<NavigationsController> context,
            ILanguageService languageService,
            INavigationService navigationService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new IndexViewModel
            {
                NavigationRoles = await _navigationService.GetNavigationRolesAsync()
            };

            var topLevelNavigations = await _navigationService.GetTopLevelNavigationsAsync();
            viewModel.Navigations = topLevelNavigations
                .OrderByDescending(_ => _.Id == viewModel.NavigationRoles.Top)
                .ThenByDescending(_ => _.Id == viewModel.NavigationRoles.Middle)
                .ThenByDescending(_ => _.Id == viewModel.NavigationRoles.Left)
                .ThenByDescending(_ => _.Id == viewModel.NavigationRoles.Footer)
                .ToList();

            foreach (var navigation in viewModel.Navigations)
            {
                navigation.SubnavigationCount = await _navigationService
                    .GetSubnavigationCountAsnyc(navigation.Id);
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

            viewModel.OpenRoles = openRoles;

            return View(viewModel);
        }

        [Route("{id}")]
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

            if (viewModel.RoleProperties.CanHaveText)
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
                    childNavigation.NavigationLanguages = await _navigationService
                        .GetNavigationLanguagesByIdAsync(childNavigation.Id);

                    if (viewModel.RoleProperties.CanHaveGrandchildren)
                    {
                        childNavigation.SubnavigationCount = await _navigationService
                            .GetSubnavigationCountAsnyc(childNavigation.Id);
                    }
                }

                viewModel.Navigations = childNavigations;
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(DetailsViewModel model)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _navigationService.SetNavigationTextAsync(model.NavigationText);
                    ShowAlertSuccess("Updated navigation text");
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

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteText(DetailsViewModel model)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService
                .GetActiveByIdAsync(model.NavigationText.LanguageId);

            if (language.IsDefault)
            {
                ShowAlertDanger("Cannot delete text for the default language");
            }
            else
            {
                try
                {
                    await _navigationService.DeleteNavigationTextAsync(
                        model.NavigationText.NavigationId,
                        model.NavigationText.LanguageId);

                    ShowAlertSuccess($"Deleted {language.Description} navigation text");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting navigation text: {Message}", ex.Message);
                    ShowAlertDanger($"Error deleting {language.Description} navigation text");
                }
            }

            return RedirectToAction(nameof(Details), new
            {
                id = model.NavigationText.NavigationId,
                language = language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
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

        [HttpPost]
        [Route("[action]")]
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


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteNavigation(Navigation navigation)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
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

        [HttpPost]
        [Route("[action]")]
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
    }
}
