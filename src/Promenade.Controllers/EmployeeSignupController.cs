using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Controllers.ViewModels.EmployeeSignup;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility;
using Ocuda.Utility.Filters;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public partial class EmployeeSignupController : BaseController<EmployeeSignupController>
    {
        private readonly EmployeeCardService _employeeCardService;
        private readonly SegmentService _segmentService;
        public EmployeeSignupController(ServiceFacades.Controller<EmployeeSignupController> context,
            EmployeeCardService employeeCardService,
            SegmentService segmentService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(employeeCardService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _employeeCardService = employeeCardService;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "EmployeeSignup"; } }

        [HttpGet]
        [RestoreModelState]
        public async Task<IActionResult> Index()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var viewModel = new EmployeeSignupViewModel()
            {
                Departments = new SelectList(await _employeeCardService.GetDepartmentsAsync(),
                    nameof(EmployeeCardDepartment.Id),
                    nameof(EmployeeCardDepartment.Name))
            };

            var segmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.EmployeeSignup.HomeSegment,
                forceReload);
            if (segmentId > 0)
            {
                viewModel.SegmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    segmentId,
                    forceReload);
                if (viewModel.SegmentText == null)
                {
                    _logger.LogError("Employee signup 'Home' segment id {SegmentId} not found",
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

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Index(EmployeeSignupViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (viewModel.ExistingAccount
                && string.IsNullOrWhiteSpace(viewModel.CardRequest.CardNumber))
            {
                ModelState.AddModelError("CardRequest.CardNumber",
                    _localizer[ErrorMessage.FieldRequired,
                        _localizer[i18n.Keys.Promenade.PromptLibraryCardNumber]]);
            }

            string phoneNumber = viewModel.CardRequest.Phone;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                phoneNumber = DigitsOnlyRegex().Replace(viewModel.CardRequest.Phone, string.Empty);
                if (phoneNumber.Length != 10)
                {
                    ModelState.AddModelError(
                        $"{nameof(viewModel.CardRequest)}.{nameof(viewModel.CardRequest.Phone)}",
                        _localizer[i18n.Keys.Promenade.ErrorTelephoneFormat]);
                }
            }

            if (!string.IsNullOrEmpty(viewModel.CardRequest.ZipCode)
                && (!long.TryParse(viewModel.CardRequest.ZipCode, out long _)
                    || viewModel.CardRequest.ZipCode.Length != 5))
            {
                ModelState.AddModelError(
                        $"{nameof(viewModel.CardRequest)}.{nameof(viewModel.CardRequest.ZipCode)}",
                        _localizer[i18n.Keys.Promenade.ErrorZipCode]);
            }

            var employeeNumberFormat = await _siteSettingService.GetSettingStringAsync(
                Models.Keys.SiteSetting.EmployeeSignup.EmployeeNumberFormat);
            if (!string.IsNullOrWhiteSpace(employeeNumberFormat)
                && !Regex.IsMatch(viewModel.CardRequest.EmployeeNumber, employeeNumberFormat))
            {
                ModelState.AddModelError(
                    $"{nameof(viewModel.CardRequest)}.{nameof(viewModel.CardRequest.EmployeeNumber)}",
                        _localizer[i18n.Keys.Promenade.InvalidFieldFormat,
                            i18n.Keys.Promenade.PromptEmployeeNumber]);
            }

            if (ModelState.IsValid)
            {
                if (!viewModel.ExistingAccount)
                {
                    viewModel.CardRequest.CardNumber = null;
                }

                viewModel.CardRequest.Phone = phoneNumber.Insert(6, "-").Insert(3, "-");
                await _employeeCardService.AddRequestAsync(viewModel.CardRequest);

                return RedirectToAction(nameof(Submitted));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Submitted()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var segmentId = await _siteSettingService.GetSettingIntAsync(
                Models.Keys.SiteSetting.EmployeeSignup.SubmittedSegment,
                forceReload);

            SegmentText segmentText = null;
            if (segmentId > 0)
            {
                segmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(segmentId, forceReload);

                if (segmentText == null)
                {
                    _logger.LogError("Employee signup 'Submitted' segment id {SegmentId} not found",
                        segmentId);
                }
                else if (!string.IsNullOrWhiteSpace(segmentText.Text))
                {
                    segmentText.Text = CommonMarkConverter.Convert(segmentText.Text);
                }
            }

            return View(segmentText);
        }

        [GeneratedRegex("[^0-9.]")]
        private static partial Regex DigitsOnlyRegex();
    }
}
