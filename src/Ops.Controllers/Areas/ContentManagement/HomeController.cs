using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Home;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area(nameof(ContentManagement))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _permissionGroupService = permissionGroupService;
        }

        public static string Area
        {
            get
            {
                return nameof(ContentManagement);
            }
        }

        public static string Name
        { get { return "Home"; } }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new IndexViewModel
            {
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                HasRosterPermissions = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.RosterManagement),
                HasUserSyncPermissions = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.UserSync)
            };

            var claimPermissionIds = UserClaims(ClaimType.PermissionId);

            if (claimPermissionIds?.Count > 0)
            {
                var ids = claimPermissionIds
                    .Select(_ => int.Parse(_, CultureInfo.InvariantCulture));
                viewModel.HasSectionManagerPermissions = await _permissionGroupService
                    .HasAPermissionAsync<PermissionGroupSectionManager>(ids);
            }

            return View(viewModel);
        }
    }
}