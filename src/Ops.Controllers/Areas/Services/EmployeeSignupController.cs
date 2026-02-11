using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Services.ViewModels.EmployeeSignup;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Filters;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Ocuda.Ops.Controllers.Areas.Services
{
    [Area("Services")]
    [Route("[area]/[controller]")]
    public class EmployeeSignupController : BaseController<EmployeeSignupController>
    {
        private readonly IEmployeeCardRequestService _employeeCardRequestService;
        private readonly IEmployeeCardService _employeeCardService;
        private readonly IPolarisHelper _polarisHelper;

        public EmployeeSignupController(ServiceFacades.Controller<EmployeeSignupController> context,
            IEmployeeCardRequestService employeeCardRequestService,
            IEmployeeCardService employeeCardService,
            IPolarisHelper polarisHelper)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(employeeCardRequestService);
            ArgumentNullException.ThrowIfNull(employeeCardService);
            ArgumentNullException.ThrowIfNull(polarisHelper);

            _employeeCardRequestService = employeeCardRequestService;
            _employeeCardService = employeeCardService;
            _polarisHelper = polarisHelper;
        }

        public static string Name
        { get { return "EmployeeSignup"; } }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Pending(int id)
        {
            var request = await _employeeCardRequestService.GetRequestAsync(id);

            if (request == null)
            {
                ShowAlertWarning($"Unable to find employee card request id {id}");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PendingViewModel
            {
                APIConfigured = _polarisHelper.IsConfigured,
                CardNumber = request.CardNumber,
                CardRequest = request,
                Note = await _employeeCardService.GetRequestNoteAsync(id)
            };

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Pending(PendingViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (string.IsNullOrWhiteSpace(viewModel.CardNumber)
                && (viewModel.Type == EmployeeCardResult.ResultType.CardCreated
                    || viewModel.Type == EmployeeCardResult.ResultType.Processed))
            {
                ModelState.AddModelError(nameof(PendingViewModel.CardNumber),
                    "A Card Number is required to complete the request.");
            }

            if (ModelState.IsValid)
            {
                await _employeeCardService.SetRequestNote(viewModel.Note);

                if (viewModel.Type != null)
                {
                    EmployeeCardResult cardResult;
                    try
                    {
                        cardResult = await _employeeCardService.ProcessRequestAsync(
                            viewModel.RequestId,
                            viewModel.CardNumber,
                            viewModel.Type.Value);
                    }
                    catch (OcudaException oex)
                    {
                        ShowAlertDanger(oex.Message);
                        return RedirectToAction(nameof(Pending), new { id = viewModel.RequestId });
                    }

                    if (cardResult.Type == EmployeeCardResult.ResultType.CardCreated)
                    {
                        _logger.LogInformation("Employee card Polaris account created for request {RequestId} by {ProcessedBy}",
                            cardResult.EmployeeCardRequestId,
                            cardResult.ProcessedBy);

                        ShowAlertSuccess($"Polaris account created for {cardResult.FirstName} {cardResult.LastName} ({cardResult.CardNumber}), the request has been marked as processed and an email has been sent!");
                    }
                    else if (cardResult.Type == EmployeeCardResult.ResultType.Processed)
                    {
                        _logger.LogInformation("Employee card request {RequestId} processed by {ProcessedBy}",
                            cardResult.EmployeeCardRequestId,
                            cardResult.ProcessedBy);

                        ShowAlertSuccess($"Request by {cardResult.FirstName} {cardResult.LastName} ({cardResult.CardNumber}) marked as processed and an email has been sent!");
                    }
                    else if (cardResult.Type == EmployeeCardResult.ResultType.ProcessedNoEmail)
                    {
                        _logger.LogInformation("Employee card request {RequestId} marked as processed with no email by {ProcessedBy}",
                            cardResult.EmployeeCardRequestId,
                            cardResult.ProcessedBy);

                        ShowAlertWarning($"Marked request {cardResult.EmployeeCardRequestId} processed without sending an email. Please contact the customer directly!");
                    }

                    return RedirectToAction(nameof(Processed),
                        new { id = cardResult.EmployeeCardRequestId });
                }
            }

            return RedirectToAction(nameof(Pending), new { id = viewModel.RequestId });
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Processed(int id)
        {
            var result = await _employeeCardService.GetResultAsync(id);

            if (result == null)
            {
                ShowAlertWarning($"Unable to find employee card request id {id}");
                return RedirectToAction(nameof(Index), new { processed = "true" });
            }

            var viewModel = new ProcessedViewModel
            {
                CardResult = result,
                Note = await _employeeCardService.GetRequestNoteAsync(id)
            };

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> Processed(ProcessedViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (ModelState.IsValid)
            {
                await _employeeCardService.SetRequestNote(viewModel.Note);
            }

            return RedirectToAction(nameof(Processed),
                new { id = viewModel.Note.EmployeeCardRequestId });
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, bool processed)
        {
            page ??= 1;

            var filter = new BaseFilter(page.Value);

            var viewModel = new IndexViewModel
            {
                CurrentPage = page.Value,
                IsProcessed = processed,
                ItemsPerPage = filter.Take.Value
            };

            if (processed)
            {
                var results = await _employeeCardService.GetResultsAsync(filter);
                viewModel.CardResults = results.Data;
                viewModel.ItemCount = results.Count;
                viewModel.PendingCount = await _employeeCardRequestService.GetRequestCountAsync();
                viewModel.ProcessedCount = results.Count;

                foreach (var request in viewModel.CardResults)
                {
                    request.DepartmentName = await _employeeCardRequestService
                        .GetDepartmentNameAsync(request.DepartmentId);
                }
            }
            else
            {
                var requests = await _employeeCardRequestService.GetRequestsAsync(filter);
                viewModel.CardRequests = requests.Data;
                viewModel.ItemCount = requests.Count;
                viewModel.PendingCount = requests.Count;
                viewModel.ProcessedCount = await _employeeCardService.GetResultCountAsync();
            }

            return View(viewModel);
        }
    }
}
