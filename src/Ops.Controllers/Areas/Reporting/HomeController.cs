using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Reporting.ViewModel;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Keys;
using Serilog.Context;

namespace Ocuda.Ops.Controllers.Areas.Reporting
{
    [Area(nameof(Reporting))]
    [Route("[area]")]
    public class HomeController(Controller<HomeController> context,
        IPermissionGroupService permissionGroupService,
        IReportingService reportingService,
        IUserService userService)
        : BaseController<HomeController>(context)
    {
        public static string Area
        {
            get { return nameof(Reporting); }
        }

        public static string Name
        {
            get { return "Home"; }
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{reportId}/{permissionGroupId}")]
        public async Task<IActionResult> AddPermissionGroup(string reportId, int permissionGroupId)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            try
            {
                await permissionGroupService.AddToPermissionGroupAsync<PermissionGroupReporting>(
                    report.InternalId,
                    permissionGroupId);
                AlertInfo = "Permission to access report added.";
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Error updating permission: {oex.Message}");
            }

            return RedirectToAction(nameof(Permissions), new { reportId });
        }

        [HttpGet("[action]")]
        public IActionResult Definition(string reportId)
        {
            if (string.IsNullOrEmpty(reportId))
            {
                return StatusCode(404);
            }

            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            return report == null ? StatusCode(404) : View(report);
        }

        [HttpGet("[action]/{reportId}")]
        public async Task<IActionResult> Details(string reportId, int page)
        {
            var currentPage = page > 0 ? page : 1;

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            report = await PopulatePermissionsAsync(permissionGroupService, report);

            if (!report.IsPermittedView)
            {
                return RedirectToUnauthorized();
            }

            report.IsPermittedImport = report.CanBeImported && report.IsPermittedImport;

            var filter = new BaseFilter<string>(currentPage, itemsPerPage)
            {
                Data = report.Id,
            };

            var collectionWithCount = await reportingService.GetDetailsAsync(filter);

            var viewModel = new DetailsViewModel
            {
                BackLink = report.ReportType switch
                {
                    ReportDefinitions.ReportTypeDigitalLibrary =>
                        Url.Action(nameof(SiteManagement.EmediaController.Index),
                                SiteManagement.EmediaController.Name,
                                new { SiteManagement.EmediaController.Area }),
                    ReportDefinitions.ReportTypeEmployeeCards =>
                        Url.Action(nameof(Services.EmployeeSignupController.Index),
                            Services.EmployeeSignupController.Name,
                            new { Services.EmployeeSignupController.Area }),
                    ReportDefinitions.ReportTypeOnlineCardRenewal =>
                        Url.Action(nameof(Services.RenewCardController.Index),
                            Services.RenewCardController.Name,
                            new { Services.RenewCardController.Area }),
                    _ => null
                },
                CurrentPage = currentPage,
                Heading = $"Results for {report.Name}",
                ItemCount = collectionWithCount.Count,
                ItemsPerPage = filter.Take.Value,
                Report = report,
            };

            viewModel.TimespanTotals.AddRange(collectionWithCount.Data);

            if (report.IsPermittedImport)
            {
                viewModel.Navigations.Add("Import Data", "import");
            }

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { currentPage = viewModel.LastPage ?? 1 });
            }

            SetPageTitle(viewModel.Heading);

            return report.Period switch
            {
                ReportDefinitionPeriod.Yearly => View("DetailsYearly", viewModel),
                ReportDefinitionPeriod.Monthly => View("DetailsMonthly", viewModel),
                _ => UnprocessableEntity(),
            };
        }

        [HttpGet("[action]/{reportId}/{year}")]
        public async Task<IActionResult> Display(string reportId, int year)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            report = await PopulatePermissionsAsync(permissionGroupService, report);
            if (!report.IsPermittedView)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var viewModel = await GetReportResultsAsync(report, new DateTime(year, 1, 1));

                if (viewModel != null)
                {
                    return View(viewModel);
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogWarning(oex, "Report display failed: {ErrorMessage}", oex.Message);
            }

            return StatusCode(404);
        }

        [HttpGet("[action]/{reportId}/{year}/{month}")]
        public async Task<IActionResult> Display(string reportId, int year, int month)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            report = await PopulatePermissionsAsync(permissionGroupService, report);
            if (!report.IsPermittedView)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var viewModel = await GetReportResultsAsync(report, new DateTime(year, month, 1));

                if (viewModel != null)
                {
                    return View(viewModel);
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogWarning(oex, "Report display failed: {ErrorMessage}", oex.Message);
            }

            return StatusCode(404);
        }

        [HttpGet("[action]/{reportId}/{year}")]
        public async Task<IActionResult> Export(string reportId, int year)
        {
            return await Export(reportId, year, null);
        }

        [HttpGet("[action]/{reportId}/{year}/{month}")]
        public async Task<IActionResult> Export(string reportId, int year, int? month)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            report = await PopulatePermissionsAsync(permissionGroupService, report);
            if (!report.IsPermittedView)
            {
                return RedirectToUnauthorized();
            }

            IEnumerable<DisplayReport> results = null;
            try
            {
                results = await reportingService.GetResultsAsync(new ReportCriteria
                {
                    Report = report,
                    StartDate = new DateTime(year, month ?? 1, 1),
                });
            }
            catch (OcudaException oex)
            {
                _logger.LogWarning(oex, "Report display failed: {ErrorMessage}", oex.Message);
            }

            if (results?.Count() > 0)
            {
                results.First().Title = report.Name;

                var ms = ExcelExportHelper.GenerateWorkbook(results,
                    new Dictionary<string, object>
                    {
                        { "Report name", report.Name },
                        { "Type", report.ReportType },
                        { "Period", report.Period },
                        { "Timeframe", month.HasValue ? $"{month:D2}/{year:D4}" : $"{year:D4}" },
                    },
                    ExcelExportHelper.SheetCriteriaDefault);

                return new FileStreamResult(ms, ExcelExportHelper.ExcelMimeType)
                {
                    FileDownloadName = month.HasValue
                        ? $"{report.Name.Replace(' ', '-')}-{year:D4}-{month:D2}.{ExcelExportHelper.ExcelFileExtension}"
                        : $"{report.Name.Replace(' ', '-')}-{year:D4}.{ExcelExportHelper.ExcelFileExtension}",
                };
            }

            return StatusCode(404);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> HasResults(string reportId, int year, int month)
        {
            var jsonResponse = new JsonResponse
            {
                ServerResponse = true,
            };

            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);

            if (report == null)
            {
                jsonResponse.Message = "Report not found.";
            }
            else
            {
                report = await PopulatePermissionsAsync(permissionGroupService, report);

                if (!report.IsPermittedView)
                {
                    jsonResponse.Message = "Permission denied";
                }
                else
                {
                    jsonResponse.Success = await reportingService.HasResultsAsync(new ReportCriteria
                    {
                        Report = report,
                        StartDate = new System.DateTime(year, month, 1),
                    });
                    jsonResponse.Message = jsonResponse.Success
                        ? $"Data already present for {month}/{year}."
                        : $"No data for {month}/{year}, good to upload!";
                }
            }

            return Json(jsonResponse);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Import(ImportViewModel viewModel)
        {
            if (viewModel == null)
            {
                ShowAlertDanger("Unable to accept uploaded data file.");
                return RedirectToAction(nameof(Import));
            }

            var report = ReportDefinitions
                .Definitions
                .SingleOrDefault(_ => _.Id == viewModel.ReportId);

            if (report == null)
            {
                ShowAlertWarning($"Unable to find report with id: {viewModel.ReportId}");
                return RedirectToAction(nameof(Index));
            }

            report = await PopulatePermissionsAsync(permissionGroupService, report);

            if (!report.IsPermittedImport)
            {
                return RedirectToUnauthorized();
            }

            await using var stream = viewModel.DataFile.OpenReadStream();

            using (LogContext.PushProperty("ImportFilename", viewModel.DataFile.FileName))
            {
                try
                {
                    var importResult = await reportingService.ProcessImportAsync(report.Id,
                        viewModel.DataDate,
                        viewModel.DataFile.FileName,
                        stream);
                    return RedirectToAction(nameof(Notes), new
                    {
                        reportId = report.Id,
                        year = viewModel.DataDate.Year,
                        month = viewModel.DataDate.Month,
                    });
                }
                catch (OcudaException oex)
                {
                    _logger.LogWarning(oex,
                        "Unable to process import: {ErrorMessage}",
                        oex.Message);
                    ShowAlertWarning(oex.Message);
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var currentPage = page > 0 ? page : 1;

            var collectionWithCount = reportingService.GetList();

            var viewModel = new IndexViewModel
            {
                CanAdjustPermissions = IsSiteManager(),
                CurrentPage = currentPage,
                Heading = "Reporting",
                IsIndex = true,
                ItemsPerPage = await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage),
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            // loop through reports to see if the current user can import for them
            foreach (var report in collectionWithCount.Data)
            {
                var reportPermissions = await PopulatePermissionsAsync(permissionGroupService,
                    report);

                report.HasResults = await reportingService.HasResultsAsync(report.Id);
                report.IsPermittedImport = report.CanBeImported
                    && reportPermissions.IsPermittedImport;
                report.IsPermittedView = reportPermissions.IsPermittedView;
            }

            viewModel.ItemCount = collectionWithCount.Data.Count(_ => _.IsPermittedView);

            viewModel.Reports = collectionWithCount.Data
                .Where(_ => _.IsPermittedView)
                .Skip(viewModel.ItemsPerPage * (page - 1))
                .Take(viewModel.ItemsPerPage);

            SetPageTitle(viewModel.Heading);
            return View(viewModel);
        }

        [HttpGet("[action]/{reportId}/{year}/{month}")]
        public async Task<IActionResult> Notes(string reportId, int year, int month)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            report = await PopulatePermissionsAsync(permissionGroupService, report);
            if (!report.IsPermittedImport)
            {
                return RedirectToUnauthorized();
            }

            IEnumerable<ReportingImportDetails> notes = null;

            try
            {
                notes = await reportingService.GetNotesAsync(new ReportCriteria
                {
                    Report = report,
                    StartDate = new System.DateTime(year, month, 1),
                });
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Request for notes not found: {ReportId} {Month}/{Year}",
                    reportId,
                    month,
                    year);
                return StatusCode(404);
            }

            var viewModel = new NotesViewModel
            {
                BackLink = Url.Action(nameof(Details), new { reportId }),
                Heading = "Import Notes",
                Notes = notes,
                SecondaryHeading = $"{report.Name} {month}/{year}",
            };

            viewModel.Navigations.Add("Display",
                Url.Action(nameof(Display), new { reportId, year, month }));

            var userCache = new Dictionary<int, User>();
            foreach (var note in viewModel.Notes)
            {
                if (userCache.TryGetValue(note.CreatedBy, out var user))
                {
                    note.CreatedByUser = user;
                }
                else
                {
                    note.CreatedByUser = await userService
                        .GetByIdIncludeDeletedAsync(note.CreatedBy);
                    userCache.Add(note.CreatedBy, note.CreatedByUser);
                }
            }

            SetPageTitle($"{viewModel.Heading} - {viewModel.SecondaryHeading}");
            return View(viewModel);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("[action]/{reportId}")]
        public async Task<IActionResult> Permissions(string reportId)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            var viewModel = new PermissionsViewModel
            {
                Heading = "Permissions",
                Report = report,
                SecondaryHeading = report.Name,
            };

            var permissionGroups = await permissionGroupService.GetAllAsync();
            var reportPermissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupReporting>(report.InternalId);

            foreach (var permissionGroup in permissionGroups)
            {
                var permission = reportPermissions
                    .SingleOrDefault(_ => _.PermissionGroupId == permissionGroup.Id);
                if (permission == null)
                {
                    viewModel.AvailableGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
                else
                {
                    viewModel.AssignedGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                    if (permission.CanImport)
                    {
                        viewModel.ImportGroups.Add(permissionGroup.Id,
                            permissionGroup.PermissionGroupName);
                    }
                }
            }

            SetPageTitle($"{viewModel.Heading} - {viewModel.SecondaryHeading}");
            return View(viewModel);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{reportId}/{permissionGroupId}")]
        public async Task<IActionResult> RemovePermissionGroup(string reportId,
            int permissionGroupId)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            try
            {
                await permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupReporting>(report.InternalId,
                        permissionGroupId);
                AlertInfo = "Permission to access report removed.";
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Error updating permission: {oex.Message}");
            }

            return RedirectToAction(nameof(Permissions), new { reportId });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{reportId}/{permissionGroupId}")]
        public async Task<IActionResult> ToggleImportPermission(string reportId,
            int permissionGroupId)
        {
            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                return StatusCode(404);
            }

            var permissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupReporting>(report.InternalId);

            var permission = permissions.Single(_ => _.PermissionGroupId == permissionGroupId);
            permission.CanImport = !permission.CanImport;

            permissionGroupService.UpdatePermissionGroup<PermissionGroupReporting>(permission);
            await permissionGroupService.SavePermissionGroups();

            return RedirectToAction(nameof(Permissions), new { reportId = report.Id });
        }

        private async Task<DisplayViewModel> GetReportResultsAsync(
            ReportDefinition report,
            DateTime period)
        {
            IEnumerable<DisplayReport> results = null;
            try
            {
                results = await reportingService.GetResultsAsync(new ReportCriteria
                {
                    Report = report,
                    StartDate = period,
                    NumberFormat = "N0",
                });
            }
            catch (OcudaException oex)
            {
                _logger.LogWarning(oex, "Report display failed: {ErrorMessage}", oex.Message);
            }

            if (results?.Count() > 0)
            {
                var viewModel = new DisplayViewModel
                {
                    BackLink = Url.Action(nameof(Details), new { reportId = report.Id }),
                    Heading = report.Name,
                    Reports = results,
                    SecondaryHeading = report.Period == ReportDefinitionPeriod.Yearly
                        ? $"{period.Year}"
                        : $"{period.Month}/{period.Year}",
                };

                if (report.CanBeImported && report.IsPermittedImport)
                {
                    var notesLink = report.Period == ReportDefinitionPeriod.Yearly
                        ? Url.Action(nameof(Notes), new { reportId = report.Id, period.Year })
                        : Url.Action(nameof(Notes), new
                        {
                            reportId = report.Id,
                            period.Year,
                            period.Month
                        });

                    viewModel.Navigations.Add("Import Notes", notesLink);
                }

                var exportLink = report.Period == ReportDefinitionPeriod.Yearly
                    ? Url.Action(nameof(Export), new { reportId = report.Id, period.Year })
                    : Url.Action(nameof(Export), new
                    {
                        reportId = report.Id,
                        period.Year,
                        period.Month
                    });

                viewModel.Navigations.Add("Export", exportLink);

                SetPageTitle(viewModel.Heading);
                return viewModel;
            }

            return null;
        }
    }
}