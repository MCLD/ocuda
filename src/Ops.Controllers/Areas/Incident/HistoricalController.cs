using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers.Areas.Incident
{
    [Area("Incident")]
    [Route("[area]/[controller]")]
    public class HistoricalController : BaseController<HistoricalController>
    {
        private readonly IHistoricalIncidentService _historicalIncidentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionGroupService _permissionGroupService;

        public HistoricalController(ServiceFacades.Controller<HistoricalController> context,
            IHttpContextAccessor httpContextAccessor,
            IHistoricalIncidentService historicalIncidentService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _historicalIncidentService = historicalIncidentService
                ?? throw new ArgumentNullException(nameof(historicalIncidentService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Name
        { get { return "Historical"; } }

        [Route("[action]/{historicalIncidentId}")]
        public async Task<IActionResult> Details(int historicalIncidentId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.ViewAllIncidentReports);

            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var historicalIncident = await _historicalIncidentService
                .GetAsync(historicalIncidentId);

            if (historicalIncident == null)
            {
                ShowAlertDanger($"Could not find historical incident id {historicalIncidentId}");
                return RedirectToAction(nameof(Index));
            }

            return View(historicalIncident);
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ViewAllIncidentReports);

            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new Service.Filters.SearchFilter(currentPage);

            var historicalIncidents = await _historicalIncidentService.GetPaginatedAsync(filter);

            var viewModel = new HistoricalIndexViewModel
            {
                CurrentPage = currentPage,
                ItemCount = historicalIncidents.Count,
                ItemsPerPage = filter.Take.Value,
                HistoricalIncidents = historicalIncidents.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }
    }
}
