using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Areas.Incident
{
    [Area(nameof(Incident))]
    [Route("[area]/[controller]")]
    public class HistoricalController : BaseController<HistoricalController>
    {
        private const string HistoricalIncidentFolder = "historicalincidents";

        private readonly IHistoricalIncidentService _historicalIncidentService;
        private readonly IPathResolverService _pathResolver;
        private readonly IPermissionGroupService _permissionGroupService;

        public HistoricalController(ServiceFacades.Controller<HistoricalController> context,
            IHistoricalIncidentService historicalIncidentService,
            IPathResolverService pathResolver,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _historicalIncidentService = historicalIncidentService
                ?? throw new ArgumentNullException(nameof(historicalIncidentService));
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Name
        { get { return "Historical"; } }

        [HttpGet("[action]/{historicalIncidentId}")]
        public async Task<IActionResult> Details(int historicalIncidentId,
            int page,
            string searchText)
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

            var filePath = _pathResolver.GetPrivateContentFilePath(historicalIncident.Filename,
                HistoricalIncidentFolder);

            return View(new HistoricalDetailsViewModel
            {
                FileExists = System.IO.File.Exists(filePath),
                HistoricalIncident = historicalIncident,
                Page = page,
                SearchText = searchText,
                SecondaryHeading = historicalIncident.Id.ToString(CultureInfo.CurrentCulture)
            });
        }

        [HttpGet("[action]/{historicalIncidentId}")]
        [ResponseCache(NoStore = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "ControllerBase.File handles disposal (dotnet/AspNetCore.Docs#14585)")]
        public async Task<IActionResult> Download(int historicalIncidentId)
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

            var filePath = _pathResolver.GetPrivateContentFilePath(historicalIncident.Filename,
                HistoricalIncidentFolder);

            var fileName = Path.GetFileName(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"File not found in area {HistoricalIncidentFolder}: {fileName}");
                _logger.LogError("File {FileName} not found in area {FilePath}",
                    fileName,
                    HistoricalIncidentFolder);

                return RedirectToAction(nameof(Details),
                    new { historicalIncidentId });
            }

            return File(new FileStream(filePath, FileMode.Open, FileAccess.Read),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                fileName);
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page, string searchText)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ViewAllIncidentReports);

            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new Service.Filters.SearchFilter(currentPage)
            {
                SearchText = searchText
            };

            var historicalIncidents = await _historicalIncidentService.GetPaginatedAsync(filter);

            var viewModel = new HistoricalIndexViewModel
            {
                CanViewAll = await CanViewAllAsync(),
                CurrentPage = currentPage,
                ItemCount = historicalIncidents.Count,
                ItemsPerPage = filter.Take.Value,
                HistoricalIncidents = historicalIncidents.Data,
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