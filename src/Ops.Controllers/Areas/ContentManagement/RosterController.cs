using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class RosterController : BaseController<RosterController>
    {
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IRosterService _rosterService;

        public RosterController(ServiceFacades.Controller<RosterController> context,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            IRosterService rosterService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _rosterService = rosterService
                ?? throw new ArgumentNullException(nameof(rosterService));
        }

        public static string Area
        { get { return "ContentManagement"; } }

        public static string Name
        { get { return "Roster"; } }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddMap(int unitId, int locationId, int page)
        {
            if (!await HasRosterManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            var result = await _rosterService.AddUnitMap(unitId, locationId);

            if (!string.IsNullOrEmpty(result))
            {
                ShowAlertDanger($"There was an issue adding that mapping: {result}");
            }

            if (page > 1)
            {
                return RedirectToAction(nameof(UnitMapping), new { page });
            }
            else
            {
                return RedirectToAction(nameof(UnitMapping));
            }
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        [RestoreModelState(Key = "RosterUpload")]
        public async Task<IActionResult> Index(int page)
        {
            if (!await HasRosterManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new BaseFilter(currentPage);

            var rosterHeaders = await _rosterService
                .GetPaginatedRosterHeadersAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = currentPage,
                ItemCount = rosterHeaders.Count,
                ItemsPerPage = filter.Take.Value,
                RosterHeaders = rosterHeaders.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveMap(int unitId, int page)
        {
            if (!await HasRosterManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            var result = await _rosterService.RemoveUnitMap(unitId);

            if (!string.IsNullOrEmpty(result))
            {
                ShowAlertDanger($"There was an issue adding that mapping: {result}");
            }

            if (page > 1)
            {
                return RedirectToAction(nameof(UnitMapping), new { page });
            }
            else
            {
                return RedirectToAction(nameof(UnitMapping));
            }
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> UnitMapping(int page)
        {
            if (!await HasRosterManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new BaseFilter(currentPage);

            var unitLocations = await _rosterService.GetUnitLocationMapsAsync(filter);

            var viewModel = new UnitMappingViewModel
            {
                CurrentPage = currentPage,
                ItemCount = unitLocations.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = await GetLocationsDropdownAsync(_locationService),
                UnitLocationMaps = unitLocations.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState(Key = "RosterUpload")]
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            if (!await HasRosterManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            if (model != null && ModelState.IsValid)
            {
                using (Serilog.Context.LogContext.PushProperty("RosterFileName", model.FileName))
                {
                    _logger.LogInformation("Inserting roster {FileName}", model.FileName);
                    var timer = new System.Diagnostics.Stopwatch();
                    timer.Start();
                    var tempFile = Path.GetTempFileName();
                    using (var fileStream = new FileStream(tempFile, FileMode.Create))
                    {
                        await model.Roster.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var rosterResult
                            = await _rosterService.ImportRosterAsync(CurrentUserId, tempFile);
                        timer.Stop();
                        _logger.LogInformation("Roster {FileName} processed {Count} rows in {ElapsedMs} ms",
                            model.FileName,
                            rosterResult.TotalRows,
                            timer.ElapsedMilliseconds);
                        AlertInfo = $"Processed {rosterResult.TotalRows} roster rows in {timer.Elapsed:dd\\.hh\\:mm\\:ss}";
                        return View("Changes", rosterResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error inserting roster data: {Message}", ex.Message);
                        AlertDanger = "An error occured while uploading the roster.";
                    }
                    finally
                    {
                        System.IO.File.Delete(Path.Combine(Path.GetTempPath(), tempFile));
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var filenameErrors = ModelState[nameof(model.FileName)]?.Errors?.ToList();
                if (filenameErrors?.Count > 0)
                {
                    foreach (var error in filenameErrors)
                    {
                        ModelState[nameof(model.Roster)].Errors.Add(error);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> HasRosterManagementRightsAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.RosterManagement);
        }
    }
}
