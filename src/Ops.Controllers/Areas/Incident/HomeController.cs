using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.Incident
{
    [Area(nameof(Incident))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IIncidentService _incidentService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(Controller<HomeController> context,
            IIncidentService incidentService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            _incidentService = incidentService
                ?? throw new ArgumentNullException(nameof(incidentService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Name
        { get { return "Home"; } }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> Add()
        {
            var viewModel = new AddViewModel
            {
                IncidentTypes = await GetIncidentTypesDropdownAsync(),
                Locations = await GetLocationsDropdownAsync(_locationService)
            };
            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> Add(AddViewModel viewModel)
        {
            return RedirectToAction(nameof(Mine));
        }

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

            var filter = new IncidentFilter(currentPage)
            {
                SearchText = searchText
            };

            var viewModel = await GetIncidentsAsync(filter, currentPage);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.ViewingAll = true;
            viewModel.SecondaryHeading = nameof(All);

            return View("Index", viewModel);
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Mine(int page, string searchText)
        {
            int currentPage = page != 0 ? page : 1;

            var filter = new IncidentFilter(currentPage)
            {
                CreatedById = CurrentUserId,
                SearchText = searchText,
            };

            var viewModel = await GetIncidentsAsync(filter, currentPage);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.SecondaryHeading = nameof(Mine);

            return View("Index", viewModel);
        }

        private async Task<bool> CanViewAllAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ViewAllIncidentReports);
        }

        private async Task<IndexViewModel> GetIncidentsAsync(IncidentFilter filter, int currentPage)
        {
            var incidents = await _incidentService.GetPaginatedAsync(filter);

            return new IndexViewModel
            {
                CanViewAll = await CanViewAllAsync(),
                CurrentPage = currentPage,
                Incidents = incidents.Data,
                ItemCount = incidents.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = await _locationService.GetAllLocationsIdNameAsync(),
                SearchText = filter.SearchText,
            };
        }

        private async Task<IEnumerable<SelectListItem>> GetIncidentTypesDropdownAsync()
        {
            var incidentTypes = await _incidentService.GetIncidentTypesAsync();
            return incidentTypes.Select(_ => new SelectListItem
            {
                Value = _.Key.ToString(CultureInfo.InvariantCulture),
                Text = _.Value
            });
        }
    }
}
