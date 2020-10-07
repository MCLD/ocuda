using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.CoverIssue.ViewModels.Management;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.CoverIssue
{
    [Area("CoverIssue")]
    [Route("[area]")]
    public class ManagementController : BaseController<ManagementController>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ICoverIssueService _coverIssueService;
        private readonly IPermissionGroupService _permissionGroupService;

        public static string Name { get { return "Management"; } }
        
        private const string BookmarkletFilePath = "js/coverissue-bookmarklet.min.js";

        public ManagementController(ServiceFacades.Controller<ManagementController> context,
            IWebHostEnvironment hostingEnvironment,
            ICoverIssueService coverIssueService,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            _hostingEnvironment = hostingEnvironment
                ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _coverIssueService = coverIssueService
                ?? throw new ArgumentNullException(nameof(coverIssueService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1, CoverIssueType? type = null)
        {
            var filter = new CoverIssueFilter(page)
            {
                CoverIssueType = type ?? CoverIssueType.Open
            };
            var headers = await _coverIssueService.GetPaginatedHeaderListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = headers.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new
                {
                    page = paginateModel.LastPage ?? 1
                });
            }

            var viewModel = new IndexViewModel
            {
                CoverIssueHeaders = headers.Data,
                PaginateModel = paginateModel,
                LeapPath = await _siteSettingService.GetSettingStringAsync(Models
                    .Keys
                    .SiteSetting
                    .CoverIssueReporting
                    .LeapBibUrl)
            };

            return View(viewModel);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var header = await _coverIssueService.GetHeaderByIdAsync(id);
            if (header == null)
            {
                _logger.LogWarning("Cover issue report {CoverIssueReportId} could not be found.",
                    id);
                ShowAlertDanger("The requested report could not be found.");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DetailViewModel
            {
                Header = header,
                Details = await _coverIssueService.GetDetailsByHeaderIdAsync(header.Id),
                CanEdit = await HasCoverIssueManagementPermissionAsync()
            };

            var leapBibUrl = await _siteSettingService.GetSettingStringAsync(Models
                .Keys
                .SiteSetting
                .CoverIssueReporting
                .LeapBibUrl);

            if (!string.IsNullOrWhiteSpace(leapBibUrl))
            {
                viewModel.LeapPath = leapBibUrl + header.BibId;
            }

            return View(viewModel);
        }
        [Route("[action]/{bibId}")]
        public async Task<IActionResult> Report(int bibId)
        {
            try
            {
                await _coverIssueService.AddCoverIssueAsync(bibId);
                var header = await _coverIssueService.GetHeaderByBibIdAsync(bibId);
                ShowAlertSuccess("The cover issue has been reported!");
                return RedirectToAction(nameof(ManagementController.Details),
                    new { id = header.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error recording cover issue for BibId {BibId}: {Message}",
                    bibId,
                    ex.Message);
                ShowAlertDanger("There was an error reporting this cover issue.");
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResolveIssue(DetailViewModel model)
        {
            if (!await HasCoverIssueManagementPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var header = await _coverIssueService.GetHeaderByIdAsync(model.HeaderId);

                await _coverIssueService.ResolveCoverIssueAsnyc(header.Id);
                ShowAlertSuccess("Issue marked as resolved!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error resolving cover issue for header {HeaderId}: {Message}",
                    model.HeaderId,
                    ex.Message);
                ShowAlertDanger("An error occured while trying to make the issue as resolved");
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("[action]")]
        public async Task<JsonResult> GetBookmarklet()
        {
            try
            {
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath,
                    BookmarkletFilePath);
                using (var sr = new StreamReader(filePath))
                {
                    var baseUrl = Url.Action(nameof(Report), Name, new { bibId = 0 }, Request.Scheme);
                    baseUrl = baseUrl.TrimEnd('0');
                    var bookmarklet = await sr.ReadToEndAsync();
                    bookmarklet = bookmarklet.Replace("{0}", baseUrl).Trim();
                    return Json(new { success = true, bookmarklet });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Could not retrieve bookmarklet, contact an administrator." });
            }
        }

        private async Task<bool> HasCoverIssueManagementPermissionAsync()
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    var permissionGroups = await _permissionGroupService
                        .GetApplicationPermissionGroupsAsync(
                            ApplicationPermission.CoverIssueManagement);
                    var permissionGroupsStrings = permissionGroups
                        .Select(_ => _.Id.ToString(CultureInfo.InvariantCulture));

                    return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                }
                return false;
            }
        }
    }
}