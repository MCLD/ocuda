using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Services.ViewModels.CardRenewal;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Controllers.Areas.Services
{
    [Area("Services")]
    [Route("[area]/[controller]")]
    public class CardRenewalController : BaseController<CardRenewalController>
    {
        private readonly ICardRenewalRequestService _cardRenewalRequestService;
        private readonly ICardRenewalService _cardRenewalService;
        private readonly IPolarisHelper _polarisHelper;

        public CardRenewalController(ServiceFacades.Controller<CardRenewalController> context,
            ICardRenewalRequestService cardRenewalRequestService,
            ICardRenewalService cardRenewalService,
            IPolarisHelper polarisHelper)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(cardRenewalRequestService);
            ArgumentNullException.ThrowIfNull(cardRenewalService);
            ArgumentNullException.ThrowIfNull(polarisHelper);

            _cardRenewalRequestService = cardRenewalRequestService;
            _cardRenewalService = cardRenewalService;
            _polarisHelper = polarisHelper;
        }

        public static string Name
        { get { return "CardRenewal"; } }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _cardRenewalRequestService.GetRequestAsync(id);

            if (request == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var patronData = _polarisHelper.GetPatronDataOverride(request.Barcode);

            var viewModel = new DetailsViewModel
            {
                AddressLookupPath = await _siteSettingService.GetSettingStringAsync(Models
                    .Keys
                    .SiteSetting
                    .CardRenewal
                    .AddressLookupUrl),
                AssessorLookupPath = await _siteSettingService.GetSettingStringAsync(Models
                    .Keys
                    .SiteSetting
                    .CardRenewal
                    .AssessorLookupUrl),
                PatronCode = await _polarisHelper.GetPatronCodeNameAsync(patronData.PatronCodeID),
                PatronData = patronData,
                PatronName = $"{patronData.NameFirst} {patronData.NameLast}",
                Request = request
            };

            if (!viewModel.Request.ProcessedAt.HasValue)
            {
                var responses = await _cardRenewalService.GetAvailableResponsesAsync();
                viewModel.ResponseList = responses.Select(_ => new SelectListItem
                {
                    Text = _.Name,
                    Value = _.Id.ToString(CultureInfo.InvariantCulture)
                });
            }
            else
            {
                viewModel.Result = await _cardRenewalService
                    .GetResultForRequestAsync(request.Id);
            }

            var acceptedCounty = await _siteSettingService.GetSettingStringAsync(Models
                .Keys
                .SiteSetting
                .CardRenewal
                .AcceptedCounty);

            if (!string.IsNullOrWhiteSpace(acceptedCounty))
            {
                viewModel.AcceptedCounty = acceptedCounty;
                viewModel.InCounty = patronData.PatronAddresses.Any(_ =>
                    string.Equals(_.County, acceptedCounty, StringComparison.OrdinalIgnoreCase));
            }

            var juvenilePatronCodes = await _siteSettingService.GetSettingStringAsync(Models
                .Keys
                .SiteSetting
                .CardRenewal
                .JuvenilePatronCodes);

            if (!string.IsNullOrWhiteSpace(juvenilePatronCodes))
            {
                var patronCodeList = juvenilePatronCodes
                            .Split(",", StringSplitOptions.RemoveEmptyEntries
                                | StringSplitOptions.TrimEntries)
                            .ToList();

                foreach (var patronCode in patronCodeList)
                {
                    int patronCodeId;

                    if (int.TryParse(patronCode, out patronCodeId))
                    {
                        if (patronData.PatronCodeID == patronCodeId)
                        {
                            viewModel.IsJuvenile = true;

                            if (patronData.BirthDate.HasValue
                                && patronData.BirthDate.Value != DateTime.MinValue)
                            {
                                DateTime today = DateTime.Today;
                                var age = today.Year - patronData.BirthDate.Value.Year;
                                if (patronData.BirthDate.Value > today.AddYears(-age))
                                {
                                    age--;
                                }
                                viewModel.PatronAge = age;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"Invalid juvenile patron code id '{patronCode}'");
                    }
                }
            }

            var leapPatronUrl = await _siteSettingService.GetSettingStringAsync(Models
                .Keys
                .SiteSetting
                .CardRenewal
                .LeapPatronUrl);

            if (!string.IsNullOrWhiteSpace(leapPatronUrl))
            {
                viewModel.LeapPath = leapPatronUrl + request.PatronId;
            }

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> Details(DetailsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    var request = await _cardRenewalRequestService
                        .GetRequestAsync(viewModel.RequestId);

                    if (request.ProcessedAt.HasValue) 
                    {
                        _logger.LogError($"Attempted to process request {request.Id} which has already been processed.");
                    }


                    var result = _polarisHelper.RenewPatronRegistration(
                        request.Barcode,
                        request.Email);

                    await _cardRenewalService.ProcessRequestAsync(
                        request.Id,
                        viewModel.ResponseId.Value,
                        viewModel.ResponseText,
                        viewModel.PatronName);
                }
                catch (OcudaException ex)
                {

                }
            }

            return RedirectToAction(nameof(Details), new
            {
                id = viewModel.RequestId
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Discard(int id)
        {
            try
            {
                await _cardRenewalService.DiscardRequestAsync(id);
                ShowAlertSuccess($"Request {id} has been discarded");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to discard request: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]")]
        public async Task<JsonResult> GetResponseText(int responseId, int languageId)
        {
            try
            {
                var response = await _cardRenewalService
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
            page ??= 1;

            var filter = new RequestFilter(page.Value)
            {
                IsProcessed = processed
            };

            var requests = await _cardRenewalRequestService.GetRequestsAsync(filter);

            var viewModel = new IndexViewModel
            {
                CardRequests = requests.Data,
                CurrentPage = page.Value,
                IsProcessed = processed,
                ItemCount = requests.Count,
                ItemsPerPage = filter.Take.Value
            };

            if (processed)
            {
                viewModel.PendingCount = await _cardRenewalRequestService.GetRequestCountAsync(false);
                viewModel.ProcessedCount = viewModel.ItemCount;
            }
            else
            {
                viewModel.PendingCount = viewModel.ItemCount;
                viewModel.ProcessedCount = await _cardRenewalRequestService.GetRequestCountAsync(true);
            }

            return View(viewModel);
        }
    }
}
