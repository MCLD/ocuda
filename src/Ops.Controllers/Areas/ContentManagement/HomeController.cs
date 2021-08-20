using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Home;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Name { get { return "Home"; } }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new IndexViewModel
            {
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                HasDigitalDisplayPermissions = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.DigitalDisplayContentManagement)
            };

            if (!viewModel.HasPermissions)
            {
                AlertWarning = "It appears that you do not have any Intranet Administration permissions. Please contact your system administrator for more information.";
            }

            return View(viewModel);
        }
    }
}