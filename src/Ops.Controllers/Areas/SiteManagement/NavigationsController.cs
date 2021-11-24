using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
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
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.NavigationManagement))
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new IndexViewModel
            {
                TopNavigationId = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdTop),
                MiddleNavigationId = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdMiddle),
                LeftNavigationId = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdLeft),
                FooterNavigationId = await _siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Site.NavigationIdFooter)
            };

            var topLevelNavs = await _navigationService.GetTopLevelNavigationsAsync();
            viewModel.TopLevelNavigations = topLevelNavs
                .OrderByDescending(_ => _.Id == viewModel.TopNavigationId)
                .ThenByDescending(_ => _.Id == viewModel.MiddleNavigationId)
                .ThenByDescending(_ => _.Id == viewModel.LeftNavigationId)
                .ThenByDescending(_ => _.Id == viewModel.FooterNavigationId)
                .ThenBy(_ => _.Name)
                .ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> CreateTopLevelNavigation(IndexViewModel model)
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

                    navigation = await _navigationService.CreateAsync(model.Navigation);
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

        [Route("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            return View();
        }
    }
}
