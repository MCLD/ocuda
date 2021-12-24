using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models.Navigation;
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
        private readonly ISiteSettingPromService _siteSettingPromService;

        public static string Name { get { return "Navigations"; } }
        public static string Area { get { return "SiteManagement"; } }

        public NavigationsController(ServiceFacades.Controller<NavigationsController> context,
            ILanguageService languageService,
            INavigationService navigationService,
            IPermissionGroupService permissionGroupService,
            ISiteSettingPromService siteSettingPromService)
            : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _siteSettingPromService = siteSettingPromService
                ?? throw new ArgumentNullException(nameof(siteSettingPromService));
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
        public async Task<IActionResult> Detail(int id)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new IndexViewModel();

            viewModel.Navigation = await _navigationService.GetByIdAsync(id);

            if (viewModel.Navigation == null)
            {
                return RedirectToAction(nameof(Index));
            }

            viewModel.Navigations = await _navigationService
                .GetNavigationChildrenAsync(id);

            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreatePrimaryNavigation(IndexViewModel model)
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
                    var navigation = model.Navigation;
                    navigation.ChangeToLinkWhenExtraSmall = false;
                    navigation.HideTextWhenExtraSmall = false;
                    navigation.TargetNewWindow = false;
                    navigation.Icon = null;

                    navigation = await _navigationService.CreateAsync(model.Navigation, model.Role);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Index), new { id = navigation.Id })
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
        public async Task<IActionResult> EditTopLevelNavigation(IndexViewModel model)
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
                    var navigation = model.Navigation;
                    navigation.ChangeToLinkWhenExtraSmall = false;
                    navigation.HideTextWhenExtraSmall = false;
                    navigation.TargetNewWindow = false;
                    navigation.Icon = null;

                    navigation = await _navigationService.EditAsync(model.Navigation);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Index), new { id = navigation.Id })
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
        public async Task<IActionResult> DeleteNavigation(IndexViewModel model)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _navigationService.DeleteAsync(model.Navigation.Id);
                ShowAlertSuccess($"Deleted navigation: {model.Navigation.Name}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to delete navigation: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting navigation: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting navigation: {model.Navigation.Name}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
