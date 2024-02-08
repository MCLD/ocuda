using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.CoverIssue.ViewModels.Management;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.CoverIssue
{
    [Area("CoverIssue")]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private static readonly string BookmarkletFilePath = Path.Combine("Scripts",
            "CoverIssue-Bookmarklet.js");

        private readonly ICoverIssueService _coverIssueService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
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

        public static string Name
        { get { return "Home"; } }

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
                CanEdit = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.CoverIssueManagement)
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

        [Route("[action]")]
        public async Task<JsonResult> GetBookmarklet()
        {
            try
            {
                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath,
                    BookmarkletFilePath);
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("Unable to find bookmarklet template at {filePath}",
                        filePath);
                }
                var unminifiedBookmarklet = await System.IO.File.ReadAllTextAsync(filePath);
                var baseUrl = Url.Action(nameof(Report), Name, new { bibId = 0 }, Request.Scheme);
                baseUrl = baseUrl.TrimEnd('0');
                var bookmarklet = NUglify.Uglify.Js(unminifiedBookmarklet.Replace("{0}",
                    baseUrl,
                    StringComparison.OrdinalIgnoreCase)).Code;
                return Json(new { success = true, bookmarklet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred creating the bookmarklet: {ErrorMessage}",
                    ex.Message);
            }
            {
                return Json(new { success = false, message = "Could not retrieve bookmarklet, contact an administrator." });
            }
        }

        [Route("")]
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

        [Route("[action]/{bibId}")]
        public async Task<IActionResult> Report(int bibId)
        {
            try
            {
                await _coverIssueService.AddCoverIssueAsync(bibId);
                var header = await _coverIssueService.GetHeaderByBibIdAsync(bibId);
                ShowAlertSuccess("The cover issue has been reported!");
                return RedirectToAction(nameof(HomeController.Details),
                    new { id = header.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error recording cover issue for BibId {BibId}: {Message}",
                    bibId,
                    ex.Message);
                ShowAlertDanger("There was an error reporting this cover issue.");
                return base.RedirectToAction(nameof(Controllers.HomeController.Index), Controllers.HomeController.Name);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResolveIssue(DetailViewModel model)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.CoverIssueManagement))
            {
                return RedirectToUnauthorized();
            }

            if (model != null)
            {
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
            }

            return RedirectToAction(nameof(Index));
        }
    }
}