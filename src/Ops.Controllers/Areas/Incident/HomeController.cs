using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;

namespace Ocuda.Ops.Controllers.Areas.Incident
{
    [Area(nameof(Incident))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(Controller<HomeController> context,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Name
        { get { return "Home"; } }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            var viewModel = new IndexViewModel
            {
                CanViewAll = await CanViewAllAsync()
            };

            return View(viewModel);
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> All(int page)
        {
            var viewModel = new IndexViewModel
            {
                CanViewAll = await CanViewAllAsync(),
                ViewingAll = true
            };

            return View("Index", viewModel);
        }


        private async Task<bool> CanViewAllAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ViewAllIncidentReports);
        }
    }
}
