using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Controllers.ViewModels.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class ServicesController : BaseController<ServicesController>
    {
        private readonly EmployeeCardService _employeeCardService;
        private readonly PageService _pageService;
        private readonly SegmentService _segmentService;

        public ServicesController(ServiceFacades.Controller<ServicesController> context,
            EmployeeCardService employeeCardService,
            PageService pageService,
            SegmentService segmentService)
            : base(context) 
        {
            ArgumentNullException.ThrowIfNull(employeeCardService);
            ArgumentNullException.ThrowIfNull(pageService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _employeeCardService = employeeCardService;
            _pageService = pageService;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "Services"; } }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> EmployeeCard()
        {
            var viewModel = new EmployeeCardViewModel()
            {
                CardRequest = new EmployeeCardRequest()
            };

            var employeeCardSegmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Card.EmployeeCardSegment);
            if (employeeCardSegmentId > 0)
            {
                viewModel.Segment =  await _segmentService
                    .GetSegmentTextBySegmentIdAsync(employeeCardSegmentId, false);
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EmployeeCard(EmployeeCardViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (viewModel.CardRequest.ExistingAccount
                && string.IsNullOrWhiteSpace(viewModel.CardRequest.CardNumber))
            {
                ModelState.AddModelError("CardRequest.CardNumber",
                    _localizer[ErrorMessage.FieldRequired,
                        _localizer[i18n.Keys.Promenade.PromptLibraryCardNumber]]);
            }

            if (ModelState.IsValid)
            {
                if (!viewModel.CardRequest.ExistingAccount)
                {
                    viewModel.CardRequest.CardNumber = null;
                }

                await _employeeCardService.AddRequestAsync(viewModel.CardRequest);

                var thanksPageId = await _siteSettingService.GetSettingIntAsync(
                    Models.Keys.SiteSetting.Card.EmployeeCardThanksPage);

                if (thanksPageId >= 0)
                {
                    var pageStub = await _pageService
                    .GetStubByHeaderIdTypeAsync(thanksPageId,
                        PageType.Thanks,
                        false);

                    if (!string.IsNullOrEmpty(pageStub))
                    {
                        return RedirectToAction(nameof(ThanksController.Page),
                            ThanksController.Name,
                            new { stub = pageStub });
                    }
                }

                SetAlertInfo(Ocuda.i18n.Keys.Promenade.EmployeeCardThankYou);
                ModelState.Clear();
            }

            return RedirectToAction(nameof(EmployeeCard));
        }
    }
}
