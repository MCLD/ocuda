﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Home;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IPermissionGroupService _permissionGroupService;

        public static string Name { get { return "Home"; } }

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new IndexViewModel
            {
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                HasPagePermissions = false
            };

            var permissionIds = UserClaims(ClaimType.PermissionId);

            if (permissionIds?.Count > 0)
            {
                var numericPermissionIds = permissionIds
                    .Select(_ => int.Parse(_, CultureInfo.InvariantCulture))
                    .ToArray();
                viewModel.HasPagePermissions = await _permissionGroupService
                    .HasPermissionAsync<PermissionGroupPageContent>(numericPermissionIds);
                viewModel.HasPodcastPermissions = await _permissionGroupService
                    .HasPermissionAsync<PermissionGroupPodcastItem>(numericPermissionIds);
            }

            if (!viewModel.IsSiteManager && !viewModel.HasPagePermissions)
            {
                return RedirectToUnauthorized();
            }

            return View(viewModel);
        }
    }
}
