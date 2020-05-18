using System;
using System.Collections.Generic;
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
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Helpers;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class HelpController : BaseController<HelpController>
    {
        private const string TempDataDateTime = "ScheduleDateTime";
        private const string TempDataSubjectId = "ScheduleSubjectId";

        private const string RequiredEmail = "The Email field is required.";
        private const string RequiredNotes = "Please help us out by telling us a little about your request.";

        private const string ErrorAvailableDays = "This service is only available on the following days: {0}";
        private const string ErrorDateOnAfter = "You must request a date on or after: {0}";
        private const string ErrorFurthestDate = "The furthest date you can schedule a call is: {0}";
        private const string ErrorEarliestTime = "The earliest time you can select is: {0}";
        private const string ErrorTimeBefore = "You must request a time before: {0}";
        private const string ErrorTelephoneFormat = "Please enter a telephone number in the format: ###-###-####";

        private const string ViewModelTelephoneId = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.ScheduleRequestTelephoneId);
        private const string ViewModelEmail = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.Email);
        private const string ViewModelNotes = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.Notes);

        private const double StartHour = 8.5;
        private const double AvailableHours = 8;
        private const double BufferHours = 4;
        private readonly IList<DayOfWeek> BlockedDays = new List<DayOfWeek>
        {
            DayOfWeek.Sunday,
            DayOfWeek.Saturday
        };

        private static readonly TimeSpan QuantizeSpan = TimeSpan.FromMinutes(30);

        private readonly ScheduleService _scheduleService;
        private readonly SegmentService _segmentService;

        public HelpController(ServiceFacades.Controller<HelpController> context,
            ScheduleService scheduleService,
            SegmentService segmentService)
            : base(context)
        {
            _scheduleService = scheduleService
                ?? throw new ArgumentNullException(nameof(scheduleService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        private DateTime FirstAvailable(DateTime date)
        {
            var firstAvailable = date.RoundUp(QuantizeSpan);

            if (firstAvailable.TimeOfDay < firstAvailable.Date.AddHours(StartHour).TimeOfDay)
            {
                firstAvailable = firstAvailable.Date.AddHours(StartHour);
            }
            else
            {
                firstAvailable = firstAvailable.AddHours(BufferHours);
            }

            if (firstAvailable.TimeOfDay
                > firstAvailable.Date.AddHours(StartHour + AvailableHours).TimeOfDay)
            {
                firstAvailable = firstAvailable.Date.AddDays(1).AddHours(StartHour);
            }

            return firstAvailable.DayOfWeek switch
            {
                DayOfWeek.Saturday => firstAvailable.Date.AddDays(2).AddHours(StartHour),
                DayOfWeek.Sunday => firstAvailable.Date.AddDays(1).AddHours(StartHour),
                _ => firstAvailable
            };
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Schedule()
        {
            return await DisplayScheduleFormAsync(null);
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> ScheduleDetails()
        {
            return await DisplayScheduleDetailsFormAsync(null);
        }

        [HttpPost("Schedule")]
        public async Task<IActionResult> SubmitSchedule(ScheduleViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var firstAvailable = FirstAvailable(DateTime.Now);

            if (BlockedDays.Contains(viewModel.RequestedDate.DayOfWeek))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorAvailableDays,
                        new DayOfWeekHelper().ListDays(BlockedDays)));
            }

            if (viewModel.RequestedDate.Date < firstAvailable.Date)
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorDateOnAfter,
                        firstAvailable.ToShortDateString()));
                viewModel.RequestedDate = firstAvailable.Date;
            }
            else if (viewModel.RequestedDate.Date > firstAvailable.Date.AddDays(7))
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorFurthestDate,
                        firstAvailable.AddDays(7).ToShortDateString()));
                viewModel.RequestedDate = firstAvailable.Date.AddDays(7);
            }

            if (viewModel.RequestedTime.TimeOfDay < firstAvailable.TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    string.Format(CultureInfo.CurrentCulture,
                        ErrorEarliestTime,
                        firstAvailable.ToShortTimeString()));
                viewModel.RequestedTime = firstAvailable.ToLocalTime();
            }
            else if (viewModel.RequestedTime.TimeOfDay
                > firstAvailable.AddHours(AvailableHours).TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorTimeBefore,
                        firstAvailable.AddHours(AvailableHours).ToShortTimeString()));
                viewModel.RequestedTime = firstAvailable.AddHours(AvailableHours).ToLocalTime();
            }

            if (ModelState.IsValid)
            {
                // store stuff in tempdata
                TempData[TempDataDateTime] = viewModel.RequestedDate
                    + viewModel.RequestedTime.TimeOfDay;
                TempData[TempDataSubjectId] = viewModel.SubjectId;

                // redirect to second form
                return RedirectToAction(nameof(ScheduleDetails));
            }
            else
            {
                return await DisplayScheduleFormAsync(viewModel);
            }
        }

        [HttpPost("ScheduleDetails")]
        public async Task<IActionResult> SubmitScheduleDetails(ScheduleDetailsViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var subjects = await _scheduleService.GetSubjectsAsync(false);

            if (ModelState.ContainsKey(ViewModelTelephoneId))
            {
                ModelState.Remove(ViewModelTelephoneId);
            }

            viewModel.ScheduleRequest.ScheduleRequestSubjectId
                = (int)TempData.Peek(TempDataSubjectId);
            viewModel.ScheduleRequest.RequestedTime
                = (DateTime)TempData.Peek(TempDataDateTime);

            string phoneNumbers;
            if (!string.IsNullOrEmpty(viewModel.ScheduleRequestPhone))
            {
                phoneNumbers = Regex.Replace(viewModel.ScheduleRequestPhone, "[^0-9.]", "");
                if (phoneNumbers.Length != 10)
                {
                    ModelState.AddModelError(nameof(viewModel.ScheduleRequestPhone),
                        ErrorTelephoneFormat);
                }
            }

            var selectedSubject = subjects
                .SingleOrDefault(_ => _.Id == viewModel.ScheduleRequest.ScheduleRequestSubjectId);

            if (selectedSubject.RequireEmail
                && string.IsNullOrWhiteSpace(viewModel.ScheduleRequest.Email))
            {
                ModelState.AddModelError(ViewModelEmail, RequiredEmail);
            }

            if (selectedSubject.RequireComments
                && string.IsNullOrWhiteSpace(viewModel.ScheduleRequest.Notes))
            {
                ModelState.AddModelError(ViewModelNotes, RequiredNotes);
            }

            if (ModelState.IsValid)
            {
                viewModel.ScheduleRequest
                    = await _scheduleService.AddAsync(viewModel.ScheduleRequest,
                        viewModel.ScheduleRequestPhone);

                var segmentId = await _siteSettingService
                        .GetSettingIntAsync(Models.Keys.SiteSetting.Scheduling.ScheduledSegment);

                if (segmentId >= 0)
                {
                    viewModel.SegmentText = await _segmentService
                        .GetSegmentTextBySegmentIdAsync(segmentId, false);

                    if (!string.IsNullOrEmpty(viewModel.SegmentText?.Text))
                    {
                        viewModel.SegmentText.Text
                            = CommonMarkConverter.Convert(viewModel.SegmentText.Text);
                    }
                }

                TempData.Remove(TempDataDateTime);
                TempData.Remove(TempDataSubjectId);

                return View("Scheduled", viewModel);
            }
            else
            {
                return await DisplayScheduleDetailsFormAsync(viewModel);
            }
        }

        private async Task<IActionResult> DisplayScheduleFormAsync(ScheduleViewModel viewModel)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var enabled = await _siteSettingService
                .GetSettingBoolAsync(Models.Keys.SiteSetting.Scheduling.Enable, forceReload);

            if (!enabled)
            {
                return await NoScheduleAsync(forceReload);
            }

            var subjects = await _scheduleService.GetSubjectsAsync(forceReload);

            if (!subjects.Any())
            {
                _logger.LogWarning("Help/Schedule is enabled but no subjects are present in the database.");
                return await NoScheduleAsync(forceReload);
            }

            var scheduleViewModel = viewModel;

            if (scheduleViewModel == null)
            {
                scheduleViewModel = new ScheduleViewModel();
                if (TempData.ContainsKey(TempDataDateTime))
                {
                    var dateTime = TempData[TempDataDateTime] as DateTime?;
                    if (dateTime != null)
                    {
                        scheduleViewModel.RequestedDate = dateTime.Value.Date;
                        scheduleViewModel.RequestedTime = dateTime.Value;
                    }
                }

                if (TempData.ContainsKey(TempDataSubjectId))
                {
                    scheduleViewModel.SubjectId = (int)TempData[TempDataSubjectId];
                }
            }

            int segmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Scheduling.EnabledSegment,
                    forceReload);

            if (segmentId != default)
            {
                scheduleViewModel.SegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(segmentId, forceReload);

                if (!string.IsNullOrEmpty(scheduleViewModel.SegmentText?.Text))
                {
                    scheduleViewModel.SegmentText.Text
                        = CommonMarkConverter.Convert(scheduleViewModel.SegmentText.Text);
                }
            }

            scheduleViewModel.Subjects = subjects.Select(_ => new SelectListItem
            {
                Text = _.Subject,
                Value = _.Id.ToString(CultureInfo.InvariantCulture)
            });

            var firstAvailable = FirstAvailable(DateTime.Now);

            if (scheduleViewModel.RequestedDate == DateTime.MinValue)
            {
                scheduleViewModel.RequestedDate = firstAvailable.Date;
            }

            if (scheduleViewModel.RequestedTime == DateTime.MinValue)
            {
                scheduleViewModel.RequestedTime = firstAvailable.ToLocalTime();
            }

            return View("Schedule", scheduleViewModel);
        }

        private async Task<IActionResult>
            DisplayScheduleDetailsFormAsync(ScheduleDetailsViewModel viewModel)
        {
            var enabled = await _siteSettingService
                .GetSettingBoolAsync(Models.Keys.SiteSetting.Scheduling.Enable);

            if (!enabled)
            {
                RedirectToAction(nameof(Schedule));
            }

            var subjects = await _scheduleService.GetSubjectsAsync(false);

            if (!subjects.Any())
            {
                RedirectToAction(nameof(Schedule));
            }

            var scheduleViewModel = viewModel ?? new ScheduleDetailsViewModel();

            int selectedSubjectId = (int)TempData.Peek(TempDataSubjectId);

            var subject = subjects.SingleOrDefault(_ => _.Id == selectedSubjectId);

            if (subject.RequireEmail)
            {
                scheduleViewModel.EmailRequiredMessage = RequiredEmail;
            }

            if (subject.RequireComments)
            {
                scheduleViewModel.NotesRequiredMessage = RequiredNotes;
            }

            int segmentId = subject.SegmentId ?? await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.Scheduling.EnabledSegment);

            if (segmentId >= 0)
            {
                scheduleViewModel.SegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(segmentId, false);

                if (!string.IsNullOrEmpty(scheduleViewModel.SegmentText?.Text))
                {
                    scheduleViewModel.SegmentText.Text
                        = CommonMarkConverter.Convert(scheduleViewModel.SegmentText.Text);
                }
            }

            scheduleViewModel.DisplaySubject = subject.Subject;

            scheduleViewModel.ScheduleRequest = new ScheduleRequest
            {
                RequestedTime = (DateTime)TempData.Peek(TempDataDateTime)
            };

            return View("ScheduleDetails", scheduleViewModel);
        }

        private async Task<IActionResult> NoScheduleAsync(bool forceReload)
        {
            int segmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Scheduling.DisabledSegment,
                    forceReload);

            if (segmentId != default)
            {
                return View("NoSchedule", await _segmentService
                    .GetSegmentTextBySegmentIdAsync(segmentId, forceReload));
            }

            return View("NoSchedule", null);
        }
    }
}
