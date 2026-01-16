using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Filters;
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
        public async Task<IActionResult> AdjustMapping(MappingViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Index));
            }

            int mappingId = viewModel.IsClear ? viewModel.ClearId : viewModel.UpdateId;
            int? locationId = viewModel.IsClear ? null : viewModel.SelectedLocation;

            if (viewModel.IsDivision)
            {
                await _rosterService.UpdateDivisionMappingAsync(mappingId, locationId);
                return RedirectToAction(nameof(MapDivisions));
            }
            else
            {
                await _rosterService.UpdateLocationMappingAsync(mappingId, locationId);
                return RedirectToAction(nameof(MapLocations));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ApplyChanges(int rosterHeaderId)
        {
            RosterComparison changes = null;
            var timer = Stopwatch.StartNew();
            try
            {
                changes = await _rosterService.CompareAsync(rosterHeaderId, true);
                ShowAlertSuccess("Applied this roster to the user database");
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Error applying roster: {ErrorMessage}", oex.Message);
                ShowAlertDanger($"One or more errors occurred applying the roster: {oex.Message}.");
            }
            timer.Stop();
            return RedirectToAction(nameof(Changes), new { rosterHeaderId });
        }

        [HttpGet("[action]/{rosterHeaderId}")]
        public async Task<IActionResult> Changes(int rosterHeaderId)
        {
            RosterComparison changes = null;
            var timer = Stopwatch.StartNew();
            try
            {
                changes = await _rosterService.CompareAsync(rosterHeaderId, false);
            }
            catch (OcudaException oex)
            {
                _logger.LogWarning(oex, "Error comparing roster: {ErrorMessage}", oex.Message);
                ShowAlertWarning($"One or more errors occurred comparing the roster: {oex.Message}");
            }

            timer.Stop();
            var viewModel = new ChangesViewModel
            {
                Deactivated = changes.RemovedUsers,
                Elapsed = timer.Elapsed,
                Locations = await _locationService.GetAllLocationsIdNameAsync(),
                New = changes.NewUsers,
                NewDivisions = changes.NewDivisions,
                NewLocations = changes.NewLocations,
                RemovedDivisions = changes.RemovedDivisions,
                RemovedLocations = changes.RemovedLocations,
                RosterHeader = changes.RosterHeader,
                TotalRows = changes.TotalRecords,
                Verified = changes.UpdatedUsers
            };
            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DisableHeader(int rosterHeaderId)
        {
            await _rosterService.DisableHeaderAsync(rosterHeaderId);

            return RedirectToAction(nameof(Index));
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

        [HttpGet("[action]")]
        public async Task<IActionResult> MapDivisions()
        {
            return View("Mapping", new MappingViewModel
            {
                IsDivision = true,
                Locations = await _locationService.GetAllLocationsIdNameAsync(),
                Mapping = await _rosterService.GetDivisionsAsync(),
                Summary = "Divisions"
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> MapLocations()
        {
            return View("Mapping", new MappingViewModel
            {
                Locations = await _locationService.GetAllLocationsIdNameAsync(),
                Mapping = await _rosterService.GetLocationsAsync(),
                Summary = "Locations"
            });
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
                    var timer = System.Diagnostics.Stopwatch.StartNew();
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
                        var alert = new StringBuilder($"Imported {rosterResult.TotalRows} rows in {timer.Elapsed} ms");
                        if (rosterResult.Issues?.Count > 0)
                        {
                            alert.Append("<ul>");
                            foreach (var item in rosterResult.Issues)
                            {
                                alert.Append("<li>").Append(item).Append("</li>");
                            }
                            alert.Append("</ul>");
                            ShowAlertWarning(alert.ToString());
                        }
                        else
                        {
                            ShowAlertInfo(alert.ToString());
                        }
                        return RedirectToAction(nameof(Changes), new { rosterHeaderId = rosterResult.RosterHeaderId });
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