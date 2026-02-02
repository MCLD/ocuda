using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.PolarisHelper;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Controllers.ViewModels.RenewCard;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Filters;

namespace Ocuda.Promenade.Controllers
{
    [Route("renew-card")]
    [Route("{culture:cultureConstraint?}/renew-card")]
    public class RenewCardController : BaseController<RenewCardController>
    {
        private const int AgeOfMajority = 18;

        private const string TempDataRequest = "TempData.Request";
        private const string TempDataTimeout = "TempData.Timeout";
        private const string TempDataUnableToRenew = "TempData.UnableToRenew";

        private readonly RenewCardService _renewCardService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPolarisHelper _polarisHelper;
        private readonly SegmentService _segmentService;

        public RenewCardController(ServiceFacades.Controller<RenewCardController> context,
            RenewCardService renewCardService,
            IDateTimeProvider dateTimeProvider,
            IPolarisHelper polarisHelper,
            SegmentService segmentService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(renewCardService);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(polarisHelper);
            ArgumentNullException.ThrowIfNull(segmentService);

            _renewCardService = renewCardService;
            _dateTimeProvider = dateTimeProvider;
            _polarisHelper = polarisHelper;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "RenewCard"; } }

        [HttpGet]
        [RestoreModelState]
        public async Task<IActionResult> Index(string cardNumber)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            if (!_polarisHelper.IsConfigured)
            {
                _logger.LogError("Card renewal API settings are not configured");

                var notConfiguredSegmentId = await _siteSettingService.GetSettingIntAsync(
                    Models.Keys.SiteSetting.RenewCard.NotConfiguredSegment,
                    forceReload);

                SegmentText notConfiguredSegmentText = null;
                if (notConfiguredSegmentId > 0)
                {
                    notConfiguredSegmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                        notConfiguredSegmentId,
                        forceReload);
                    if (notConfiguredSegmentText == null)
                    {
                        _logger.LogError("Card renewal 'Not configured' segment id {SegmentId} not found",
                            notConfiguredSegmentId);
                    }
                    else if (!string.IsNullOrWhiteSpace(notConfiguredSegmentText.Text))
                    {
                        notConfiguredSegmentText.Text = CommonMarkConverter.Convert(
                            notConfiguredSegmentText.Text);
                    }
                }

                return View("NotConfigured", notConfiguredSegmentText);
            }

            if (TempData.ContainsKey(TempDataTimeout))
            {
                ModelState.AddModelError(nameof(IndexViewModel.Invalid),
                    _localizer[i18n.Keys.Promenade.RenewCardSessionTimeout]);
                TempData.Remove(TempDataTimeout);
            }

            var viewModel = new IndexViewModel()
            {
                Barcode = cardNumber,
                ForgotPasswordLink = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.Site.ForgotPasswordLink,
                    forceReload)
            };

            var renewCardSegmentId = await _siteSettingService.GetSettingIntAsync(
                Models.Keys.SiteSetting.RenewCard.HomeSegment,
                forceReload);
            if (renewCardSegmentId > 0)
            {
                viewModel.SegmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    renewCardSegmentId,
                    forceReload);
                if (viewModel.SegmentText == null)
                {
                    _logger.LogError("Card renewal 'Home' segment id {SegmentId} not found",
                        renewCardSegmentId);
                }
                else if (!string.IsNullOrWhiteSpace(viewModel.SegmentText.Text))
                {
                    viewModel.SegmentText.Text = CommonMarkConverter.Convert(
                        viewModel.SegmentText.Text);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Index(IndexViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (!_polarisHelper.IsConfigured)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var barcode = string.Concat(viewModel.Barcode.Where(_ => !char.IsWhiteSpace(_)));
                var password = viewModel.Password.Trim();

                try
                {
                    var validateResult = _polarisHelper.AuthenticateCustomer(barcode, password);

                    if (!validateResult)
                    {
                        _logger.LogInformation("Invalid card number or password for Barcode {Barcode}",
                            barcode);
                        ModelState.AddModelError(nameof(viewModel.Invalid),
                            _localizer[i18n.Keys.Promenade.RenewCardInvalidLogin]);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (OcudaException)
                {
                    ModelState.AddModelError(nameof(viewModel.Invalid),
                            _localizer[i18n.Keys.Promenade.ErrorProcessingRequest]);
                    return RedirectToAction(nameof(Index));
                }

                Customer customer;
                try
                {
                    customer = _polarisHelper.GetCustomerData(barcode, password);

                    if (customer == null)
                    {
                        _logger.LogError("Unable to find customer record for the barcode {Barcode}", barcode);
                        ModelState.AddModelError(nameof(viewModel.Invalid),
                            _localizer[i18n.Keys.Promenade.ErrorProcessingRequest]);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (OcudaException)
                {
                    ModelState.AddModelError(nameof(viewModel.Invalid),
                            _localizer[i18n.Keys.Promenade.ErrorProcessingRequest]);
                    return RedirectToAction(nameof(Index));
                }

                // Handle accounts with a pending request
                var pendingRequest = await _renewCardService
                    .GetPendingRequestAsync(customer.Id);
                if (pendingRequest != null)
                {
                    ModelState.AddModelError(nameof(viewModel.Invalid),
                        _localizer[i18n.Keys.Promenade.RenewCardPendingRequest,
                        pendingRequest.SubmittedAt.ToString("d", CultureInfo.CurrentCulture),
                        pendingRequest.SubmittedAt.ToString("t", CultureInfo.CurrentCulture)]);
                    return RedirectToAction(nameof(Index));
                }

                // Handle accounts that aren't expiring soon
                var cutoffDays = await _siteSettingService.GetSettingIntAsync(
                    Models.Keys.SiteSetting.RenewCard.ExpirationCutoffDays);
                if (cutoffDays > -1)
                {
                    var renewalAllowedDate = customer
                        .ExpirationDate.Value.AddDays(-cutoffDays).Date;
                    if (renewalAllowedDate > _dateTimeProvider.Now.Date)
                    {
                        ModelState.AddModelError(nameof(viewModel.Invalid),
                            _localizer[i18n.Keys.Promenade.RenewCardInvalidCutoff,
                                cutoffDays,
                                renewalAllowedDate.ToShortDateString()]);
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Handle accounts belonging to staff
                var staffCustomerCodes = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.RenewCard.StaffCustomerCodes);
                if (!string.IsNullOrWhiteSpace(staffCustomerCodes))
                {
                    var customerCodeList = staffCustomerCodes
                        .Split(",", StringSplitOptions.RemoveEmptyEntries
                            | StringSplitOptions.TrimEntries)
                        .ToList();

                    foreach (var customerCode in customerCodeList)
                    {
                        if (int.TryParse(customerCode, out int customerCodeId))
                        {
                            if (customer.CustomerCodeId == customerCodeId)
                            {
                                ModelState.AddModelError(nameof(viewModel.Invalid),
                                    _localizer[i18n.Keys.Promenade.RenewCardInvalidStaff]);
                                return RedirectToAction(nameof(Index));
                            }
                        }
                        else
                        {
                            _logger.LogError("Invalid staff customer code id {CustomerCode}",
                                customerCode);
                        }
                    }
                }

                // Handle accounts belonging to nonresidents 
                var nonresidentCustomerCodes = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.RenewCard.NonresidentCustomerCodes);
                if (!string.IsNullOrWhiteSpace(nonresidentCustomerCodes))
                {
                    var customerCodeList = nonresidentCustomerCodes
                        .Split(",", StringSplitOptions.RemoveEmptyEntries
                            | StringSplitOptions.TrimEntries)
                        .ToList();

                    foreach (var customerCode in customerCodeList)
                    {
                        if (int.TryParse(customerCode, out int customerCodeId))
                        {
                            if (customer.CustomerCodeId == customerCodeId)
                            {
                                var nonresidentSegmentId = await _siteSettingService
                                    .GetSettingIntAsync(Models.Keys.SiteSetting.RenewCard
                                        .NonresidentSegment);
                                if (nonresidentSegmentId <= 0)
                                {
                                    _logger.LogError("'Nonresident' segment not set for card renewal");
                                }

                                TempData[TempDataUnableToRenew] = nonresidentSegmentId;

                                return RedirectToAction(nameof(UnableToRenew));
                            }
                        }
                        else
                        {
                            _logger.LogError("Invalid nonresident customer code id {CustomerCode}",
                                customerCode);
                        }
                    }
                }

                // Handle accounts that have aged out of their code
                var ageCheckCustomerCodes = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.RenewCard.AgeCheckCustomerCodes);
                if (!string.IsNullOrWhiteSpace(ageCheckCustomerCodes))
                {
                    var customerCodeList = ageCheckCustomerCodes
                        .Split(",", StringSplitOptions.RemoveEmptyEntries
                            | StringSplitOptions.TrimEntries)
                        .ToList();

                    foreach (var customerCode in customerCodeList)
                    {
                        if (int.TryParse(customerCode, out int customerCodeId))
                        {
                            if (customer.CustomerCodeId == customerCodeId)
                            {
                                if (customer.BirthDate.HasValue
                                    && customer.BirthDate.Value != DateTime.MinValue)
                                {
                                    DateTime today = DateTime.Today;
                                    var age = today.Year - customer.BirthDate.Value.Year;
                                    if (customer.BirthDate.Value > today.AddYears(-age))
                                    {
                                        age--;
                                    }

                                    if (age >= AgeOfMajority)
                                    {
                                        var ageCheckSegmentId = await _siteSettingService
                                            .GetSettingIntAsync(Models.Keys.SiteSetting.RenewCard
                                                .AgeCheckSegment);
                                        if (ageCheckSegmentId <= 0)
                                        {
                                            _logger.LogError("'Age check' segment not set for card renewal");
                                        }

                                        TempData[TempDataUnableToRenew] = ageCheckSegmentId;

                                        return RedirectToAction(nameof(UnableToRenew));
                                    }
                                }

                                break;
                            }
                        }
                        else
                        {
                            _logger.LogError("Invalid age check customer code id {CustomerCode}",
                                customerCode);
                        }
                    }
                }

                IEnumerable<CustomerAddress> addresses = customer.Addresses;

                var acceptedCounties = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.RenewCard.AcceptedCounties);
                if (!string.IsNullOrWhiteSpace(acceptedCounties))
                {
                    var counties = acceptedCounties.Split(",",
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    addresses = addresses.Where(_ => counties.Contains(_.County,
                        StringComparer.OrdinalIgnoreCase));
                }
                addresses = addresses
                    .DistinctBy(_ => new
                    {
                        _.StreetAddressOne,
                        _.StreetAddressTwo,
                        _.City,
                        _.PostalCode
                    });

                var request = new RenewCardRequest
                {
                    Addresses = addresses,
                    Barcode = barcode,
                    CustomerId = customer.Id,
                    Email = customer.EmailAddress?.Trim()
                };

                var juvenileCustomerCodes = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.RenewCard.JuvenileCustomerCodes);
                if (!string.IsNullOrWhiteSpace(juvenileCustomerCodes))
                {
                    var customerCodeList = juvenileCustomerCodes
                        .Split(",", StringSplitOptions.RemoveEmptyEntries
                            | StringSplitOptions.TrimEntries)
                        .ToList();

                    foreach (var customerCode in customerCodeList)
                    {
                        if (int.TryParse(customerCode, out int customerCodeId))
                        {
                            if (customer.CustomerCodeId == customerCodeId)
                            {
                                request.IsJuvenile = true;
                                break;
                            }
                        }
                        else
                        {
                            _logger.LogError("Invalid juvenile customer code id {CustomerCode}",
                                customerCode);
                        }
                    }
                }

                TempData[TempDataRequest] = JsonSerializer.Serialize(request);

                if (request.IsJuvenile)
                {
                    return RedirectToAction(nameof(Juvenile));
                }

                return RedirectToAction(nameof(VerifyAddress));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> Juvenile()
        {
            if (!TempData.ContainsKey(TempDataRequest) || !_polarisHelper.IsConfigured)
            {
                if (!TempData.ContainsKey(TempDataRequest))
                {
                    TempData[TempDataTimeout] = true;
                }
                return RedirectToAction(nameof(Index));
            }

            var request = JsonSerializer.Deserialize<RenewCardRequest>(
                (string)TempData.Peek(TempDataRequest));

            if (!request.IsJuvenile)
            {
                return RedirectToAction(nameof(VerifyAddress));
            }

            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var viewModel = new JuvenileViewModel();

            var segmentId = await _siteSettingService.GetSettingIntAsync(
                Models.Keys.SiteSetting.RenewCard.JuvenileSegment,
                forceReload);

            if (segmentId > 0)
            {
                viewModel.SegmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    segmentId,
                    forceReload);
                if (viewModel.SegmentText == null)
                {
                    _logger.LogError("Card renewal 'Juvenile' segment id {SegmentId} not found",
                        segmentId);
                }
                else if (!string.IsNullOrWhiteSpace(viewModel.SegmentText.Text))
                {
                    viewModel.SegmentText.Text = CommonMarkConverter.Convert(
                        viewModel.SegmentText.Text);
                }
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public IActionResult Juvenile(JuvenileViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (!TempData.ContainsKey(TempDataRequest) || !_polarisHelper.IsConfigured)
            {
                if (!TempData.ContainsKey(TempDataRequest))
                {
                    TempData[TempDataTimeout] = true;
                }
                return RedirectToAction(nameof(Index));
            }

            var request = JsonSerializer.Deserialize<RenewCardRequest>(
                (string)TempData.Peek(TempDataRequest));

            if (!request.IsJuvenile)
            {
                return RedirectToAction(nameof(VerifyAddress));
            }

            if (ModelState.IsValid)
            {
                request.GuardianBarcode = string.Concat(viewModel.GuardianBarcode
                    .Where(_ => !char.IsWhiteSpace(_)));
                request.GuardianName = viewModel.GuardianName.Trim();
                TempData[TempDataRequest] = JsonSerializer.Serialize(request);

                return RedirectToAction(nameof(VerifyAddress));
            }

            return RedirectToAction(nameof(Juvenile));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Submitted()
        {
            if (!_polarisHelper.IsConfigured)
            {
                return RedirectToAction(nameof(Index));
            }

            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var segmentId = await _siteSettingService.GetSettingIntAsync(
                Models.Keys.SiteSetting.RenewCard.SubmittedSegment,
                forceReload);

            SegmentText segmentText = null;
            if (segmentId > 0)
            {
                segmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(segmentId, forceReload);

                if (segmentText == null)
                {
                    _logger.LogError("Card renewal 'Submitted' segment id {SegmentId} not found",
                        segmentId);
                }
                else if (!string.IsNullOrWhiteSpace(segmentText.Text))
                {
                    segmentText.Text = CommonMarkConverter.Convert(segmentText.Text);
                }
            }

            return View(segmentText);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UnableToRenew()
        {
            if (!TempData.ContainsKey(TempDataUnableToRenew) || !_polarisHelper.IsConfigured)
            {
                if (!TempData.ContainsKey(TempDataUnableToRenew))
                {
                    TempData[TempDataTimeout] = true;
                }
                return RedirectToAction(nameof(Index));
            }

            var segmentId = (int)TempData.Peek(TempDataUnableToRenew);

            SegmentText segmentText = null;
            if (segmentId > 0)
            {
                var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

                segmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                segmentId,
                forceReload);

                if (segmentText == null)
                {
                    _logger.LogError("Card renewal segment id {SegmentId} not found for unable to renew response",
                        segmentId);
                }
                else if (!string.IsNullOrWhiteSpace(segmentText.Text))
                {
                    segmentText.Text = CommonMarkConverter.Convert(segmentText.Text);
                }
            }

            return View(segmentText);
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> VerifyAddress()
        {
            if (!TempData.ContainsKey(TempDataRequest) || !_polarisHelper.IsConfigured)
            {
                if (!TempData.ContainsKey(TempDataRequest))
                {
                    TempData[TempDataTimeout] = true;
                }
                return RedirectToAction(nameof(Index));
            }

            var request = JsonSerializer.Deserialize<RenewCardRequest>(
                (string)TempData.Peek(TempDataRequest));

            if (request.IsJuvenile && string.IsNullOrWhiteSpace(request.GuardianName))
            {
                return RedirectToAction(nameof(Juvenile));
            }

            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var viewModel = new VerifyAddressViewModel()
            {
                Addresses = request.Addresses,
                Email = request.Email
            };

            var verifyAddressSegmentId = await _siteSettingService.GetSettingIntAsync(
                Models.Keys.SiteSetting.RenewCard.VerifyAddressSegment,
                forceReload);
            if (verifyAddressSegmentId > 0)
            {
                viewModel.HeaderSegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(verifyAddressSegmentId, forceReload);

                if (viewModel.HeaderSegmentText == null)
                {
                    _logger.LogError("Card renewal 'Verify address' segment id {SegmentId} not found",
                        verifyAddressSegmentId);
                }
                else if (!string.IsNullOrWhiteSpace(viewModel.HeaderSegmentText.Text))
                {
                    viewModel.HeaderSegmentText.Text = CommonMarkConverter.Convert(
                        viewModel.HeaderSegmentText.Text);
                }
            }

            if (!request.Addresses.Any())
            {
                var noAddressSegmentId = await _siteSettingService.GetSettingIntAsync(
                    Models.Keys.SiteSetting.RenewCard.NoAddressSegment,
                    forceReload);
                if (noAddressSegmentId > 0)
                {
                    viewModel.NoAddressSegmentText = await _segmentService
                        .GetSegmentTextBySegmentIdAsync(noAddressSegmentId, forceReload);

                    if (viewModel.NoAddressSegmentText == null)
                    {
                        _logger.LogError("Card renewal 'No address' segment id {SegmentId} not found",
                            noAddressSegmentId);
                    }
                    else if (!string.IsNullOrWhiteSpace(viewModel.NoAddressSegmentText.Text))
                    {
                        viewModel.NoAddressSegmentText.Text = CommonMarkConverter.Convert(
                            viewModel.NoAddressSegmentText.Text);
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> VerifyAddress(VerifyAddressViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (!TempData.ContainsKey(TempDataRequest) || !_polarisHelper.IsConfigured)
            {
                if (!TempData.ContainsKey(TempDataRequest))
                {
                    TempData[TempDataTimeout] = true;
                }
                return RedirectToAction(nameof(Index));
            }

            var request = JsonSerializer.Deserialize<RenewCardRequest>(
                (string)TempData.Peek(TempDataRequest));

            if (request.IsJuvenile && string.IsNullOrWhiteSpace(request.GuardianName))
            {
                return RedirectToAction(nameof(Juvenile));
            }

            if (ModelState.IsValid)
            {
                request.Email = viewModel.Email.Trim();

                if (viewModel.SameAddress && request.Addresses.Any())
                {
                    request.SameAddress = true;
                }

                await _renewCardService.CreateRequestAsync(request);

                TempData.Remove(TempDataRequest);

                return RedirectToAction(nameof(Submitted));
            }

            return RedirectToAction(nameof(VerifyAddress));
        }
    }
}
