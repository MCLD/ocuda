using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Help;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint}/[Controller]")]
    public class HelpController : BaseController<HelpController>
    {
        private readonly ScheduleService _scheduleService;
        private readonly SegmentService _segmentService;

        public HelpController(ServiceFacades.Controller<HelpController> context,
            ScheduleService scheduleService,
            SegmentService segmentService)
            : base(context)
        {
            _scheduleService = scheduleService
                ?? throw new System.ArgumentNullException(nameof(scheduleService));
            _segmentService = segmentService
                ?? throw new System.ArgumentNullException(nameof(segmentService));
        }

        private System.DateTime FirstAvailable()
        {
            var firstAvailable = System.DateTime.Today.AddHours(8.5);
            switch (firstAvailable.DayOfWeek)
            {
                case System.DayOfWeek.Friday:
                    firstAvailable = firstAvailable.AddDays(3);
                    break;
                case System.DayOfWeek.Saturday:
                    firstAvailable = firstAvailable.AddDays(2);
                    break;
                default:
                    firstAvailable = firstAvailable.AddDays(1);
                    break;
            }
            return firstAvailable;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Schedule()
        {
            return await DisplayScheduleForm(null);
        }

        [HttpPost("Schedule")]
        public async Task<IActionResult> SaveSchedule(ScheduleViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var firstAvailable = FirstAvailable();

            if (viewModel.RequestedDate.Date < firstAvailable.Date)
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    $"You must request a date on or after {firstAvailable.Date.ToShortDateString()}");
                viewModel.RequestedDate = firstAvailable.Date;
            }
            else if (viewModel.RequestedDate.Date > firstAvailable.Date.AddDays(7))
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    $"The furthest date you can schedule a call is {firstAvailable.Date.AddDays(7).ToShortDateString()}");
                viewModel.RequestedDate = firstAvailable.Date.AddDays(7);
            }

            if (viewModel.RequestedTime.TimeOfDay < firstAvailable.TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    $"You must request a time after {firstAvailable.ToShortTimeString()}");
                viewModel.RequestedTime = firstAvailable.ToLocalTime();
            }
            else if (viewModel.RequestedTime.TimeOfDay > firstAvailable.AddHours(8).TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    $"You must request a time before {firstAvailable.AddHours(8).ToShortTimeString()}");
                viewModel.RequestedTime = firstAvailable.AddHours(8).ToLocalTime();
            }

            var subjects = await _scheduleService.GetSubjectsAsync();

            if (ModelState.ContainsKey("ScheduleRequest.ScheduleRequestTelephoneId"))
            {
                ModelState.Remove("ScheduleRequest.ScheduleRequestTelephoneId");
            }

            string phoneNumbers;
            if (!string.IsNullOrEmpty(viewModel.ScheduleRequestPhone))
            {
                phoneNumbers = Regex.Replace(viewModel.ScheduleRequestPhone, "[^0-9.]", "");
                if (phoneNumbers.Length != 10)
                {
                    ModelState.AddModelError(nameof(viewModel.ScheduleRequestPhone),
                        "Please enter a telephone number in the format: ###-###-####");
                }
            }

            if (ModelState.IsValid)
            {
                viewModel.ScheduleRequest.RequestedTime = new System.DateTime(
                    viewModel.RequestedDate.Year,
                    viewModel.RequestedDate.Month,
                    viewModel.RequestedDate.Day,
                    viewModel.RequestedTime.Hour,
                    viewModel.RequestedTime.Minute,
                    0);

                var scheduleRequest = await _scheduleService.AddAsync(viewModel.ScheduleRequest,
                    viewModel.ScheduleRequestPhone);

                var scheduleViewModel = new ScheduleViewModel
                {
                    ScheduleRequest = scheduleRequest,
                    ScheduleRequestSubject = subjects
                        .SingleOrDefault(_ => _.Id == scheduleRequest.ScheduleRequestSubjectId)
                        .Subject
                };

                var segmentId = await _siteSettingService
                        .GetSettingIntAsync(Models.Keys.SiteSetting.Scheduling.ScheduledSegment);

                if (segmentId >= 0)
                {
                    viewModel.SegmentText
                        = await _segmentService.GetSegmentTextBySegmentIdAsync(segmentId);

                    if (!string.IsNullOrEmpty(viewModel.SegmentText?.Text))
                    {
                        viewModel.SegmentText.Text
                            = CommonMarkConverter.Convert(viewModel.SegmentText.Text);
                    }
                }

                return View("Scheduled", scheduleViewModel);
            }
            else
            {
                return await DisplayScheduleForm(viewModel);
            }
        }

        private async Task<IActionResult> DisplayScheduleForm(ScheduleViewModel viewModel)
        {
            var enabled = await _siteSettingService
                .GetSettingBoolAsync(Models.Keys.SiteSetting.Scheduling.Enable);

            var segmentSetting = enabled
                ? Models.Keys.SiteSetting.Scheduling.EnabledSegment
                : Models.Keys.SiteSetting.Scheduling.DisabledSegment;

            int segmentId = -1;

            segmentId = await _siteSettingService
                    .GetSettingIntAsync(segmentSetting);

            var scheduleViewModel = viewModel != null ? viewModel : new ScheduleViewModel();

            if (segmentId >= 0)
            {
                scheduleViewModel.SegmentText
                    = await _segmentService.GetSegmentTextBySegmentIdAsync(segmentId);

                if (!string.IsNullOrEmpty(scheduleViewModel.SegmentText?.Text))
                {
                    scheduleViewModel.SegmentText.Text
                        = CommonMarkConverter.Convert(scheduleViewModel.SegmentText.Text);
                }
            }

            if (!enabled)
            {
                return View("NoSchedule", scheduleViewModel);
            }

            var subjects = await _scheduleService.GetSubjectsAsync();

            if (!subjects.Any())
            {
                _logger.LogWarning("Help/Schedule is enabled but no subjects are present in the database.");
                return View("NoSchedule", scheduleViewModel);
            }

            scheduleViewModel.Subjects = subjects.Select(_ => new SelectListItem
            {
                Text = _.Subject,
                Value = _.Id.ToString(CultureInfo.InvariantCulture)
            });

            var firstAvailable = FirstAvailable();

            if (scheduleViewModel.RequestedDate == System.DateTime.MinValue)
            {
                scheduleViewModel.RequestedDate = firstAvailable.Date;
            }

            if (scheduleViewModel.RequestedTime == System.DateTime.MinValue)
            {
                scheduleViewModel.RequestedTime = firstAvailable.ToLocalTime();
            }

            return View("Schedule", scheduleViewModel);
        }
    }
}
