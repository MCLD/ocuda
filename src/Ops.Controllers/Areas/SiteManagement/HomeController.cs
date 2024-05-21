using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Home;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly bool _isOptimizeConfigured;
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IPermissionGroupService permissionGroupService,
            IConfiguration config)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(config);

            _permissionGroupService = permissionGroupService;

            _isOptimizeConfigured = config[Configuration.OpsImageOptimizerUsername] != null;
        }

        public static string Name
        { get { return "Home"; } }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new IndexViewModel
            {
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
            };

            var permissionIds = UserClaims(ClaimType.PermissionId);

            if (permissionIds?.Count > 0)
            {
                var numericPermissionIds = permissionIds
                    .Select(_ => int.Parse(_, CultureInfo.InvariantCulture));
                viewModel.HasEmediaPermissions = await HasAppPermissionAsync(
                    _permissionGroupService, ApplicationPermission.EmediaManagement);
                viewModel.HasFeatureManagement = await HasAppPermissionAsync(
                    _permissionGroupService, ApplicationPermission.FeatureManagement);
                viewModel.HasNavigationPermissions = await HasAppPermissionAsync(
                    _permissionGroupService, ApplicationPermission.NavigationManagement);
                viewModel.HasPagePermissions = await _permissionGroupService
                    .HasAPermissionAsync<PermissionGroupPageContent>(numericPermissionIds)
                    || await HasAppPermissionAsync(_permissionGroupService,
                        ApplicationPermission.WebPageContentManagement);
                viewModel.HasPodcastPermissions = await _permissionGroupService
                    .HasAPermissionAsync<PermissionGroupPodcastItem>(numericPermissionIds);
                viewModel.HasProductPermissions = await _permissionGroupService
                    .HasAPermissionAsync<PermissionGroupProductManager>(numericPermissionIds);
                viewModel.HasImageOptimizePermissions = _isOptimizeConfigured
                    && await HasAppPermissionAsync(_permissionGroupService,
                        ApplicationPermission.ImageOptimizer);
            }

            return View(viewModel);
        }
    }
}