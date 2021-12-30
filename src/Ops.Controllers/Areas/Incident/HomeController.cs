using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

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

        [HttpGet("[action]")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> All(int page, string searchText)
        {
            var hasPermission = await CanViewAllAsync();

            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new Service.Filters.SearchFilter(currentPage)
            {
                SearchText = searchText
            };

            //var incidents = await _incidentService.GetPaginatedAsync(filter);

            var viewModel = new IndexViewModel
            {
                CanViewAll = await CanViewAllAsync(),
                CurrentPage = currentPage,
                ItemsPerPage = filter.Take.Value,
                SearchText = searchText,
                ViewingAll = true
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View("Index", viewModel);
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page, string searchText)
        {
            int currentPage = page != 0 ? page : 1;

            var filter = new Service.Filters.SearchFilter(currentPage)
            {
                SearchText = searchText
            };

            //var incidents = await _incidentService.GetUserPaginatedAsync(filter);

            var viewModel = new IndexViewModel
            {
                CanViewAll = await CanViewAllAsync(),
                CurrentPage = currentPage,
                ItemsPerPage = filter.Take.Value,
                SearchText = searchText
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        private async Task<bool> CanViewAllAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ViewAllIncidentReports);
        }
    }
}
