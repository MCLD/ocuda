using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Services.ViewModels.RenewCard;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Filters;

namespace Ocuda.Ops.Controllers.Areas.Services
{
    [Area("Services")]
    [Route("[area]/[controller]")]
    public class RenewCardController : BaseController<RenewCardController>
    {
        private const string leapPatronRecordsPath = "/record";

        private readonly IRenewCardRequestService _renewCardRequestService;
        private readonly IRenewCardService _renewCardService;
        private readonly ILanguageService _languageService;
        private readonly IPolarisHelper _polarisHelper;

        public RenewCardController(ServiceFacades.Controller<RenewCardController> context,
            IRenewCardRequestService renewCardRequestService,
            IRenewCardService renewCardService,
            ILanguageService languageService,
            IPolarisHelper polarisHelper)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(renewCardRequestService);
            ArgumentNullException.ThrowIfNull(renewCardService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(polarisHelper);

            _renewCardRequestService = renewCardRequestService;
            _renewCardService = renewCardService;
            _languageService = languageService;
            _polarisHelper = polarisHelper;
        }

        public static string Name
        { get { return "RenewCard"; } }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id)
        {
            if (!_polarisHelper.IsConfigured)
            {
                return RedirectToAction(nameof(Index));
            }

            var request = await _renewCardRequestService.GetRequestAsync(id);

            if (request == null)
            {
                return RedirectToAction(nameof(Index));
            }

            Customer customer;
            try
            {
                customer = _polarisHelper.GetCustomerDataOverride(request.Barcode);

                if (customer == null)
                {
                    _logger.LogWarning("Unable to find Polaris record for the barcode {Barcode}",
                        request.Barcode);
                    ShowAlertDanger($"Unable to find Polaris record for the barcode '{request.Barcode}'");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
                return RedirectToAction(nameof(Index));
            }

            request.Language = await _languageService.GetActiveByIdAsync(request.LanguageId);

            var viewModel = new DetailsViewModel
            {
                AddressLookupUrlSet = !string.IsNullOrWhiteSpace(
                    await _siteSettingService.GetSettingStringAsync(Models
                        .Keys
                        .SiteSetting
                        .RenewCard
                        .AddressLookupUrl)),
                AssessorLookupUrl = await _siteSettingService.GetSettingStringAsync(Models
                    .Keys
                    .SiteSetting
                    .RenewCard
                    .AssessorLookupUrl),
                Customer = customer,
                CustomerName = $"{customer.NameFirst} {customer.NameLast}",
                Request = request
            };

            if (!request.ProcessedAt.HasValue)
            {
                var responses = await _renewCardService.GetAvailableResponsesAsync();
                viewModel.ResponseList = responses.Select(_ => new SelectListItem
                {
                    Text = _.Name,
                    Value = _.Id.ToString(CultureInfo.InvariantCulture)
                });
            }
            else
            {
                var result = await _renewCardService.GetResultForRequestAsync(request.Id);
                result.ResponseText = CommonMark.CommonMarkConverter.Convert(result.ResponseText);
                viewModel.Result = result;
            }

            try
            {
                viewModel.CustomerCode = await _polarisHelper
                    .GetCustomerCodeNameAsync(customer.CustomerCodeId);
            }
            catch (OcudaException oex)
            {
                viewModel.CustomerCodeErrorMessage = oex.Message;
            }

            try
            {
                var blocks = await _polarisHelper.GetCustomerBlocksAsync(customer.Id);
                if (blocks.Count > 0)
                {
                    var ignoredBlocks = await _siteSettingService.GetSettingStringAsync(Models
                       .Keys
                       .SiteSetting
                       .RenewCard
                       .IgnoredBlockIds);
                    if (!string.IsNullOrWhiteSpace(ignoredBlocks))
                    {
                        var ignoredBlockIdList = ignoredBlocks
                            .Split(',', StringSplitOptions.RemoveEmptyEntries
                                | StringSplitOptions.TrimEntries)
                            .ToList();

                        foreach (var ignoredBlock in ignoredBlockIdList)
                        {
                            if (int.TryParse(ignoredBlock, out int ignoredBlockId))
                            {
                                blocks.RemoveAll(_ => _.BlockId == ignoredBlockId);
                            }
                            else
                            {
                                _logger.LogError($"Invalid ignored block id '{ignoredBlock}'");
                            }
                        }
                    }

                    viewModel.CustomerBlocks = blocks;
                }
            
            }
            catch (OcudaException oex)
            {
                viewModel.CustomerBlocksErrorMessage = oex.Message;
            }

            var acceptedCounty = await _siteSettingService.GetSettingStringAsync(Models
                .Keys
                .SiteSetting
                .RenewCard
                .AcceptedCounty);
            if (!string.IsNullOrWhiteSpace(acceptedCounty))
            {
                viewModel.AcceptedCounty = acceptedCounty;
                viewModel.InCounty = customer.Addresses.Any(_ =>
                    string.Equals(_.County, acceptedCounty, StringComparison.OrdinalIgnoreCase));
            }

            var chargeLimit = await _siteSettingService.GetSettingDoubleAsync(Models
                .Keys
                .SiteSetting
                .RenewCard
                .ChargesLimit);
            if (chargeLimit >= 0 && customer.ChargeBalance >= chargeLimit)
            {

                viewModel.OverChargesLimit = true;
            }

            if (!string.IsNullOrWhiteSpace(request.GuardianName) && customer.BirthDate.HasValue
                && customer.BirthDate.Value != DateTime.MinValue)
            {
                DateTime today = DateTime.Today;
                var age = today.Year - customer.BirthDate.Value.Year;
                if (customer.BirthDate.Value > today.AddYears(-age))
                {
                    age--;
                }
                viewModel.CustomerAge = age;
            }

            var leapPatronUrl = await _siteSettingService.GetSettingStringAsync(Models
                .Keys
                .SiteSetting
                .RenewCard
                .LeapPatronUrl);
            if (!string.IsNullOrWhiteSpace(leapPatronUrl))
            {
                viewModel.LeapPath = leapPatronUrl + request.CustomerId + leapPatronRecordsPath;
            }

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(DetailsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (!_polarisHelper.IsConfigured)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var processResult = await _renewCardService.ProcessRequestAsync(
                        viewModel.RequestId,
                        viewModel.ResponseId.Value,
                        viewModel.ResponseText,
                        viewModel.CustomerName);

                    if (!processResult.EmailSent)
                    {
                        ShowAlertDanger($"There was an error sending the email for request {viewModel.RequestId}");
                    }

                    if (processResult.Type == RenewCardResponse.ResponseType.Accept)
                    {
                        ShowAlertSuccess($"Request {viewModel.RequestId} has been successfully processed and the record has been updated in Polaris!");

                        if (processResult.EmailNotUpdated)
                        {
                            ShowAlertWarning("Email was not able to be updated");
                            return RedirectToAction(nameof(Details), new { viewModel.RequestId });
                        }
                    }
                    else if (processResult.Type == RenewCardResponse.ResponseType.Partial)
                    {
                        ShowAlertSuccess($"Request {viewModel.RequestId} has been successfully processed, be sure to update the record in Polaris!");
                    }
                    else
                    {
                        ShowAlertSuccess($"Request {viewModel.RequestId} has been successfully processed");
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to process card renewal request: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Details), new
            {
                id = viewModel.RequestId
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Discard(int requestId)
        {
            try
            {
                await _renewCardService.DiscardRequestAsync(requestId);
                ShowAlertSuccess($"Request {requestId} has been discarded");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to discard request: {ex.Message}");
                return RedirectToAction(nameof(Details), new { requestId });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]")]
        public async Task<JsonResult> GetResponseText(int responseId, int languageId)
        {
            try
            {
                var response = await _renewCardService
                    .GetResponseTextAsync(responseId, languageId);

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                return Json(new { success = true, response.Text, response.Type }, options);
            }
            catch (OcudaException ex)
            {
                var response = new JsonResponse
                {
                    Message = ex.Message,
                    Success = false
                };
                return Json(response);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Index(int? page, bool processed)
        {
            if (!_polarisHelper.IsConfigured)
            {
                _logger.LogError("Card renewal API settings are not configured");
            }

            page ??= 1;

            var filter = new RequestFilter(page.Value)
            {
                IsProcessed = processed
            };

            var requests = await _renewCardRequestService.GetRequestsAsync(filter);

            var viewModel = new IndexViewModel
            {
                APIConfigured = _polarisHelper.IsConfigured,
                CardRequests = requests.Data,
                CurrentPage = page.Value,
                IsProcessed = processed,
                ItemCount = requests.Count,
                ItemsPerPage = filter.Take.Value
            };

            if (processed)
            {
                viewModel.PendingCount = await _renewCardRequestService.GetRequestCountAsync(false);
                viewModel.ProcessedCount = viewModel.ItemCount;

                foreach (var request in viewModel.CardRequests)
                {
                    request.Accepted = await _renewCardService.IsRequestAccepted(request.Id);
                }
            }
            else
            {
                viewModel.PendingCount = viewModel.ItemCount;
                viewModel.ProcessedCount = await _renewCardRequestService.GetRequestCountAsync(true);
            }

            return View(viewModel);
        }
    }
}
