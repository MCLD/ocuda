using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Services.ViewModels.EmployeeSignup;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;

namespace Ocuda.Ops.Controllers.Areas.Services
{
    [Area(nameof(Services))]
    [Route("[area]/[controller]")]
    public class EmployeeSignupController(ServiceFacades.Controller<EmployeeSignupController> context,
        IEmployeeCardRequestService employeeCardRequestService,
        IEmployeeCardService employeeCardService,
        IPermissionGroupService permissionGroupService,
        IReportingService reportingService,
        IPolarisHelper polarisHelper)
        : BaseController<EmployeeSignupController>(context)
    {
        private const string PageTitle = "Employee Card Signup";

        public static string Area
        {
            get { return nameof(Services); }
        }

        public static string Name
        {
            get { return "EmployeeSignup"; }
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, bool processed)
        {
            page ??= 1;

            var filter = new BaseFilter(page.Value);

            var viewModel = new IndexViewModel
            {
                CurrentPage = page.Value,
                HasAccess = await HasAreaPermissionAsync(),
                IsProcessed = processed,
                ItemsPerPage = filter.Take.Value,
            };

            if (processed)
            {
                var results = await employeeCardService.GetResultsAsync(filter);
                viewModel.CardResults = results.Data;
                viewModel.ItemCount = results.Count;
                viewModel.ProcessedCount = results.Count;

                if (viewModel.PastMaxPage)
                {
                    return RedirectToRoute(new
                    {
                        page = viewModel.LastPage ?? 1,
                        processed = true,
                    });
                }

                viewModel.PendingCount = await employeeCardRequestService.GetRequestCountAsync();

                foreach (var request in viewModel.CardResults)
                {
                    request.DepartmentName = await employeeCardRequestService
                        .GetDepartmentNameAsync(request.DepartmentId);
                }

                SetPageTitle($"{PageTitle} - Processed");
            }
            else
            {
                var requests = await employeeCardRequestService.GetRequestsAsync(filter);
                viewModel.CardRequests = requests.Data;
                viewModel.ItemCount = requests.Count;
                viewModel.PendingCount = requests.Count;

                if (viewModel.PastMaxPage)
                {
                    return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
                }

                viewModel.ProcessedCount = await employeeCardService.GetResultCountAsync();

                SetPageTitle($"{PageTitle} - Pending");
            }

            var reportsNoPermissions = reportingService.GetList(new BaseFilter<string>
            {
                Data = Models.Definitions.ReportDefinitions.ReportTypeEmployeeCards,
            });

            var reports = await PopulatePermissionsAsync(permissionGroupService,
                reportsNoPermissions.Data);

            viewModel.ReportLinks.AddRange(reports
                .Where(_ => _.IsPermittedView)
                .ToDictionary(k => k.Name,
                    v => Url.Action(nameof(Reporting.HomeController.Details),
                        Reporting.HomeController.Name,
                        new
                        {
                            Reporting.HomeController.Area,
                            ReportId = v.Id,
                        })));

            return View(viewModel);
        }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Pending(int id)
        {
            var request = await employeeCardRequestService.GetRequestAsync(id);

            if (request == null)
            {
                ShowAlertWarning($"Unable to find employee card request id {id}");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PendingViewModel
            {
                APIConfigured = polarisHelper.IsConfigured,
                CardNumber = request.CardNumber,
                CardRequest = request,
                HasAccess = await HasAreaPermissionAsync(),
                Renewing = !string.IsNullOrWhiteSpace(request.CardNumber),
                Note = await employeeCardService.GetRequestNoteAsync(id),
            };

            SetPageTitle($"{PageTitle} - Pending {request?.CardNumber}".Trim());
            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Pending(int id, PendingViewModel viewModel)
        {
            if (!await HasAreaPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            ArgumentNullException.ThrowIfNull(viewModel);

            viewModel.RequestId = id;

            if (viewModel.Note != null)
            {
                viewModel.Note.EmployeeCardRequestId = id;
            }

            if (string.IsNullOrWhiteSpace(viewModel.CardNumber)
                && (viewModel.Type == EmployeeCardResult.ResultType.CardCreated
                    || viewModel.Type == EmployeeCardResult.ResultType.Processed))
            {
                ModelState.AddModelError(nameof(PendingViewModel.CardNumber),
                    "A Card Number is required to complete the request.");
            }

            if (ModelState.IsValid)
            {
                await employeeCardService.SetRequestNote(viewModel.Note);

                if (viewModel.Type != null)
                {
                    try
                    {
                        var emailSent = await employeeCardService.ProcessRequestAsync(
                            viewModel.RequestId,
                            viewModel.CardNumber,
                            viewModel.Type.Value);

                        if (emailSent == false)
                        {
                            ShowAlertDanger("The email failed to send, please contact the customer directly.");
                        }

                        if (viewModel.Type == EmployeeCardResult.ResultType.CardCreated
                            || viewModel.Type == EmployeeCardResult.ResultType.Processed)
                        {
                            var successMessage = new StringBuilder();
                            if (viewModel.Type == EmployeeCardResult.ResultType.CardCreated)
                            {
                                successMessage.Append(CultureInfo.InvariantCulture,
                                    $"Polaris account created for {viewModel.CardNumber}. ");
                            }

                            successMessage.Append(CultureInfo.InvariantCulture,
                                $"Request {viewModel.RequestId} has been marked as processed");
                            if (emailSent == true)
                            {
                                successMessage.Append(" and an email has been sent.");
                            }
                            else
                            {
                                successMessage.Append('.');
                            }

                            ShowAlertSuccess(successMessage.ToString());
                        }
                        else if (viewModel.Type == EmployeeCardResult.ResultType.ProcessedNoEmail)
                        {
                            ShowAlertWarning($"Marked request {viewModel.RequestId} processed without sending an email. Please contact the customer directly.");
                        }

                        return RedirectToAction(nameof(Processed),
                            new { id = viewModel.RequestId });
                    }
                    catch (OcudaException oex)
                    {
                        ShowAlertDanger(oex.Message);
                    }
                }
            }

            return RedirectToAction(nameof(Pending), new { id = viewModel.RequestId });
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Processed(int id)
        {
            if (!await HasAreaPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            var result = await employeeCardService.GetResultAsync(id);

            if (result == null)
            {
                ShowAlertWarning($"Unable to find employee card request id {id}");
                return RedirectToAction(nameof(Index), new { processed = "true" });
            }

            var viewModel = new ProcessedViewModel
            {
                CardResult = result,
                Note = await employeeCardService.GetRequestNoteAsync(id),
            };

            SetPageTitle($"{PageTitle} - Processed {result?.CardNumber}".Trim());
            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> Processed(int id, ProcessedViewModel viewModel)
        {
            if (!await HasAreaPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            ArgumentNullException.ThrowIfNull(viewModel);

            if (viewModel.Note != null)
            {
                viewModel.Note.EmployeeCardRequestId = id;
            }

            if (ModelState.IsValid)
            {
                await employeeCardService.SetRequestNote(viewModel.Note);
            }

            return RedirectToAction(nameof(Processed),
                new { id = viewModel.Note.EmployeeCardRequestId });
        }

        private async Task<bool> HasAreaPermissionAsync()
            => await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmployeeCardAccess);
    }
}