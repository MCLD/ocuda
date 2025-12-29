using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Clc.Polaris.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.PolarisHelper;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Controllers.ViewModels.CardRenewal;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class CardRenewalController : BaseController<CardRenewalController>
    {
        private const string TempDataAddresses = "TempData.Addresses";
        private const string TempDataEmail = "TempData.Email";
        private const string TempDataJuvenile = "TempData.Juvenile";
        private const string TempDataRequest = "TempData.Request";
        private const string TempDataTimeout = "TempData.Timeout";

        private readonly CardRenewalService _cardRenewalService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPolarisHelper _polarisHelper;
        private readonly SegmentService _segmentService;

        public CardRenewalController(ServiceFacades.Controller<CardRenewalController> context,
            CardRenewalService cardRenewalService,
            IDateTimeProvider dateTimeProvider,
            IPolarisHelper polarisHelper,
            SegmentService segmentService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(cardRenewalService);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(polarisHelper);
            ArgumentNullException.ThrowIfNull(segmentService);

            _cardRenewalService = cardRenewalService;
            _dateTimeProvider = dateTimeProvider;
            _polarisHelper = polarisHelper;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "CardRenewal"; } }

        [HttpGet]
        [RestoreModelState]
        public async Task<IActionResult> Index(string cardNumber)
        {
            if (TempData.ContainsKey(TempDataTimeout))
            {
                ModelState.AddModelError(nameof(IndexViewModel.Invalid),
                    _localizer[i18n.Keys.Promenade.CardRenewalSessionTimeout]);
                TempData.Remove(TempDataTimeout);
            }

            var viewModel = new IndexViewModel()
            {
                Barcode = cardNumber,
                ForgotPasswordLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Card.ForgotPasswordLink)
            };

            var cardRenewalSegmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Card.CardRenewalSegment);
            if (cardRenewalSegmentId > 0)
            {
                viewModel.SegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(cardRenewalSegmentId, false);
            }

            return View(viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Index(IndexViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (ModelState.IsValid)
            {
                var barcode = viewModel.Barcode.Trim()
                    .Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                var password = viewModel.Password.Trim();

                var patronValidateResult = _polarisHelper.AuthenticatePatron(barcode, password);

                if (patronValidateResult == null)
                {
                    _logger.LogInformation($"Invalid card number or password for Barcode '{barcode}'");
                    ModelState.AddModelError(nameof(viewModel.Invalid),
                        _localizer[i18n.Keys.Promenade.CardRenewalInvalidLogin]);
                }
                else
                {
                    var pendingRequest = await _cardRenewalService
                        .GetPendingRequestAsync(patronValidateResult.PatronID);
                    if (pendingRequest != null)
                    {
                        // TODO
                        // Send to Pending page
                    }

                    var cutoffDays = await _siteSettingService
                        .GetSettingIntAsync(Models.Keys.SiteSetting.Card.ExpirationCutoffDays);
                    if (cutoffDays > -1)
                    {
                        var renewalAllowedDate = patronValidateResult
                            .ExpirationDate.Value.AddDays(-cutoffDays).Date;
                        if (renewalAllowedDate > _dateTimeProvider.Now.Date)
                        {
                            ModelState.AddModelError(nameof(viewModel.Invalid),
                                _localizer[i18n.Keys.Promenade.CardRenewalInvalidCutoff,
                                    cutoffDays,
                                    renewalAllowedDate.ToShortDateString()]);
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // TODO
                    // Check patron code for staff ids

                    // TODO
                    // Check patron code for non resident

                    // TODO?
                    // Check patron code for juvenile restricted

                    var patronData = _polarisHelper.GetPatronData(barcode, password);

                    IEnumerable<PatronAddress> addresses = patronData.PatronAddresses;

                    var acceptedCounties = await _siteSettingService
                        .GetSettingStringAsync(Models.Keys.SiteSetting.Card.AcceptedCounties);
                    if (!string.IsNullOrWhiteSpace(acceptedCounties))
                    {
                        var counties = acceptedCounties.Split(",",
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        addresses = addresses.Where(_ => counties.Contains(_.County,
                            StringComparer.OrdinalIgnoreCase));
                    }
                    addresses = addresses
                        .DistinctBy(_ => new { _.StreetOne, _.StreetTwo, _.City, _.PostalCode });

                    var request = new CardRenewalRequest
                    {
                        Barcode = barcode,
                        PatronId = patronData.PatronID
                    };

                    TempData[TempDataAddresses] = JsonSerializer.Serialize(addresses);
                    TempData[TempDataEmail] = patronData.EmailAddress;
                    TempData[TempDataRequest] = JsonSerializer.Serialize(request);

                    var juvenilePatronCodes = await _siteSettingService
                        .GetSettingStringAsync(Models.Keys.SiteSetting.Card.JuvenilePatronCodes,true);
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
                                if (patronValidateResult.PatronCodeID == patronCodeId)
                                {
                                    TempData[TempDataJuvenile] = true;
                                    return RedirectToAction(nameof(Juvenile));
                                }
                            }
                            else
                            {
                                _logger.LogError($"Invalid juvenile patron code id '{patronCode}'");
                            }
                        }
                    }

                    // TODO
                    // Check for unpaid fines

                    // TODO
                    // Check for juvenile turned adult

                    // TODO?
                    // Check for student turned adult



                    return RedirectToAction(nameof(VerifyAddress));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public IActionResult Juvenile()
        {
            if (!TempData.ContainsKey(TempDataJuvenile))
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public IActionResult Juvenile(JuvenileViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (!TempData.ContainsKey(TempDataJuvenile))
            {
                TempData[TempDataTimeout] = true;
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var request = JsonSerializer
                    .Deserialize<CardRenewalRequest>((string)TempData.Peek(TempDataRequest));

                request.GuardianBarcode = viewModel.GuardianBarcode;
                request.GuardianName = viewModel.GuardianName;

                TempData[TempDataRequest] = JsonSerializer.Serialize(request);

                return RedirectToAction(nameof(VerifyAddress));
            }

            return RedirectToAction(nameof(Juvenile));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Submitted()
        {
            if (!TempData.ContainsKey(TempDataRequest))
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new SubmittedViewModel()
            {
                Request = JsonSerializer
                    .Deserialize<CardRenewalRequest>((string)TempData.Peek(TempDataRequest))
            };

            var submittedSegmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Card.SubmittedSegment);
            if (submittedSegmentId > 0)
            {
                viewModel.SegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(submittedSegmentId, false);
            }

            TempData.Remove(TempDataRequest);

            return View(viewModel);
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> VerifyAddress()
        {
            if (!TempData.ContainsKey(TempDataRequest))
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new VerifyAddressViewModel()
            {
                Addresses = JsonSerializer
                    .Deserialize<List<PatronAddress>>((string)TempData.Peek(TempDataAddresses)),
                Email = (string)TempData.Peek(TempDataEmail)
            };

            var verifyAddressSegmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Card.VerifyAddressSegment);
            if (verifyAddressSegmentId > 0)
            {
                viewModel.HeaderSegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(verifyAddressSegmentId, false);
            }

            if (viewModel.Addresses.Count == 0)
            {
                var noAddressSegmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Card.NoAddressSegment);
                if (noAddressSegmentId > 0)
                {
                    viewModel.NoAddressSegmentText = await _segmentService
                        .GetSegmentTextBySegmentIdAsync(noAddressSegmentId, false);
                }
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> VerifyAddress(VerifyAddressViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (!TempData.ContainsKey(TempDataRequest))
            {
                TempData[TempDataTimeout] = true;
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var request = JsonSerializer
                    .Deserialize<CardRenewalRequest>((string)TempData.Peek(TempDataRequest));

                request.Email = viewModel.Email.Trim();

                if (viewModel.SameAddress)
                {
                    var addresses = JsonSerializer
                        .Deserialize<List<PatronAddress>>((string)TempData.Peek(TempDataAddresses));
                    if (addresses.Count > 0)
                    {
                        request.SameAddress = true;
                    }
                }
                await _cardRenewalService.CreateRequestAsync(request);

                TempData.Remove(TempDataAddresses);
                TempData.Remove(TempDataEmail);

                return RedirectToAction(nameof(Submitted));
            }

            return RedirectToAction(nameof(VerifyAddress));
        }
    }
}
