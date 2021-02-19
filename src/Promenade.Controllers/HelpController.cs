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
using Ocuda.Utility.Exceptions;
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
        private const string RequiredDate = "The Requested date field is required.";
        private const string RequiredTime = "The Requested time field is required.";

        private const string ErrorAvailableDays = "This service is only available on the following days: {0}";
        private const string ErrorDateOnAfter = "You must request a date on or after: {0}";
        private const string ErrorFurthestDate = "The furthest date you can schedule a call is: {0}";
        private const string ErrorEarliestTime = "The earliest time you can select is: {0}";
        private const string ErrorTimeBefore = "You must request a time before: {0}";
        private const string ErrorTelephoneFormat = "Please enter a telephone number in the format: ###-###-####";
        private const string ErrorDateClosed = "That date is not available for scheduling: {0}";

        private const string SessionTimeout = "Unfortunately your session has timed out. Please reselect your time and subject.";

        private const string ViewModelTelephoneId = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.ScheduleRequestTelephoneId);
        private const string ViewModelEmail = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.Email);
        private const string ViewModelNotes = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.Notes);

        private static readonly TimeSpan QuantizeSpan = TimeSpan.FromMinutes(30);

        private readonly LocationService _locationService;
        private readonly ScheduleService _scheduleService;
        private readonly SegmentService _segmentService;

        public static string Name { get { return "Help"; } }

        public HelpController(ServiceFacades.Controller<HelpController> context,
            LocationService locationService,
            ScheduleService scheduleService,
            SegmentService segmentService)
            : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _scheduleService = scheduleService
                ?? throw new ArgumentNullException(nameof(scheduleService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        private async Task<List<DayOfWeek>> GetBlockedDays()
        {
            var unavailableDays = (await _siteSettingService.GetSettingStringAsync(
                Models.Keys.SiteSetting.Scheduling.UnavailableDays))
                ?.Split(',')
                .Select(_ => _?.Trim())
                .Distinct();

            var blockedDays = new List<DayOfWeek>();

            foreach (var day in unavailableDays)
            {
                if (Enum.TryParse<DayOfWeek>(day, true, out DayOfWeek result))
                {
                    blockedDays.Add(result);
                }
                else
                {
                    _logger.LogWarning("Invalid blocked day for scheduling: {day}", day);
                }
            }

            return blockedDays;
        }

        private async Task<DateTime> FirstAvailable(DateTime date)
        {
            var blockedDays = await GetBlockedDays();

            if (blockedDays.Count >= Enum.GetNames(typeof(DayOfWeek)).Length)
            {
                _logger.LogCritical("No available days configured for scheduling.");
                throw new OcudaException("No available times for scheduling could be found");
            }

            var startHour = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.StartHour);
            var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.AvailableHours);
            var bufferHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.BufferHours);

            var firstAvailable = date.RoundUp(QuantizeSpan).AddHours(bufferHours);

            if (firstAvailable.TimeOfDay < firstAvailable.Date.AddHours(startHour).TimeOfDay)
            {
                firstAvailable = firstAvailable.Date.AddHours(startHour);
            }

            if (firstAvailable.TimeOfDay
                > firstAvailable.Date.AddHours(startHour + availableHours).TimeOfDay)
            {
                firstAvailable = firstAvailable.Date.AddDays(1).AddHours(startHour);
            }

            while (blockedDays.Contains(firstAvailable.DayOfWeek))
            {
                firstAvailable = firstAvailable.Date.AddDays(1).AddHours(startHour);
            }

            return firstAvailable;
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Schedule()
        {
            return await DisplayScheduleFormAsync(null, null);
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> ScheduleTimes()
        {
            var scheduleRequestSubjectId = TempData.Peek(TempDataSubjectId) as int?;
            var requestedTime = TempData.Peek(TempDataDateTime) as DateTime?;

            if (scheduleRequestSubjectId == null || requestedTime == null)
            {
                _logger.LogError("TempData items missing in ScheduleTime");
                return await DisplayScheduleFormAsync(null, SessionTimeout);
            }

            return await DisplayScheduleTimeFormAsync(null, (DateTime)requestedTime);
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> ScheduleDetails()
        {
            var scheduleRequestSubjectId = TempData.Peek(TempDataSubjectId) as int?;
            var requestedTime = TempData.Peek(TempDataDateTime) as DateTime?;

            if (scheduleRequestSubjectId == null || requestedTime == null)
            {
                _logger.LogError("TempData items missing in ScheduleDetails");
                return await DisplayScheduleFormAsync(null, SessionTimeout);
            }

            return await DisplayScheduleDetailsFormAsync(null,
                (int)scheduleRequestSubjectId,
                (DateTime)requestedTime);
        }

        [HttpPost("Schedule")]
        public async Task<IActionResult> SubmitSchedule(ScheduleViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Schedule));
            }

            DateTime firstAvailable;
            try
            {
                firstAvailable = await FirstAvailable(DateTime.Now);
            }
            catch (OcudaException)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var blockedDays = await GetBlockedDays();

            if (blockedDays.Contains(viewModel.RequestedDate.DayOfWeek))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorAvailableDays,
                        new DayOfWeekHelper().ListDays(blockedDays)));
            }

            // check for all-location closure on that day
            var closureReason = await _locationService
                .GetClosureInformationAsync(viewModel.RequestedDate);

            if (!string.IsNullOrEmpty(closureReason))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorDateClosed,
                        closureReason));
            }

            // verify the requested date is not before the first available or 7 days after
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

            // if the selected date is today then ensure it's not before the first available time
            if (viewModel.RequestedDate.Date == DateTime.Now.Date
                && viewModel.RequestedTime.TimeOfDay < firstAvailable.TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    string.Format(CultureInfo.CurrentCulture,
                        ErrorEarliestTime,
                        firstAvailable.ToShortTimeString()));
                viewModel.RequestedTime = firstAvailable.ToLocalTime();
            }
            else
            {
                var startHour = await _siteSettingService.GetSettingDoubleAsync(
                    Models.Keys.SiteSetting.Scheduling.StartHour);
                var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                    Models.Keys.SiteSetting.Scheduling.AvailableHours);

                // if the selected date is not today, ensure times are within the allowed range
                if (viewModel.RequestedTime.TimeOfDay
                    < firstAvailable.Date.AddHours(startHour).TimeOfDay)
                {
                    DateTime earliestTime;
                    if (firstAvailable.Date == viewModel.RequestedDate.Date)
                    {
                        earliestTime = firstAvailable;
                    }
                    else
                    {
                        earliestTime = firstAvailable.Date.AddHours(startHour);
                    }

                    ModelState.AddModelError(nameof(viewModel.RequestedTime),
                        string.Format(CultureInfo.CurrentCulture,
                            ErrorEarliestTime,
                            earliestTime.ToShortTimeString()));
                    viewModel.RequestedTime = earliestTime.ToLocalTime();
                }
                else if (viewModel.RequestedTime.TimeOfDay
                    > firstAvailable.Date.AddHours(startHour + availableHours).TimeOfDay)
                {
                    DateTime latestTime = firstAvailable.Date.AddHours(startHour + availableHours);

                    ModelState.AddModelError(nameof(viewModel.RequestedTime),
                        string.Format(CultureInfo.InvariantCulture,
                            ErrorTimeBefore,
                            latestTime.ToShortTimeString()));
                    viewModel.RequestedTime = latestTime.ToLocalTime();
                }
            }

            if (ModelState.IsValid)
            {
                var requestDateTime = viewModel.RequestedDate + viewModel.RequestedTime.TimeOfDay;

                // store stuff in tempdata
                TempData[TempDataDateTime] = requestDateTime;
                TempData[TempDataSubjectId] = viewModel.SubjectId;

                if (await _scheduleService.IsRequestOverLimitAsync(requestDateTime))
                {
                    // redirect to time form if timeslot is full
                    return RedirectToAction(nameof(ScheduleTimes));
                }
                else
                {
                    // redirect to second form
                    return RedirectToAction(nameof(ScheduleDetails));
                }
            }
            else
            {
                return await DisplayScheduleFormAsync(viewModel, null);
            }
        }

        [HttpPost(nameof(ScheduleTimes))]
        public async Task<IActionResult> SubmitScheduleTimes(ScheduleTimesViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(ScheduleTimes));
            }

            // set request date and time from a suggested option
            if (viewModel.SelectedTime.HasValue)
            {
                viewModel.RequestedDate = viewModel.SelectedTime.Value.Date;
                viewModel.RequestedTime = viewModel.SelectedTime.Value;
            }

            // re-display the time form if no valid request time
            if (!viewModel.RequestedDate.HasValue || !viewModel.RequestedTime.HasValue)
            {
                if (!viewModel.RequestedDate.HasValue)
                {
                    ModelState.AddModelError(nameof(viewModel.RequestedDate), RequiredDate);
                }
                if (!viewModel.RequestedTime.HasValue)
                {
                    ModelState.AddModelError(nameof(viewModel.RequestedDate), RequiredTime);
                }

                var requestedTime = TempData.Peek(TempDataDateTime) as DateTime?;

                if (requestedTime == null)
                {
                    _logger.LogError("TempData items missing in ScheduleTime");
                    return await DisplayScheduleFormAsync(null, SessionTimeout);
                }

                return await DisplayScheduleTimeFormAsync(null, (DateTime)requestedTime);
            }

            DateTime firstAvailable;
            try
            {
                firstAvailable = await FirstAvailable(DateTime.Now);
            }
            catch (OcudaException)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var blockedDays = await GetBlockedDays();

            if (blockedDays.Contains(viewModel.RequestedDate.Value.DayOfWeek))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorAvailableDays,
                        new DayOfWeekHelper().ListDays(blockedDays)));
            }

            // check for all-location closure on that day
            var closureReason = await _locationService
                .GetClosureInformationAsync(viewModel.RequestedDate.Value);

            if (!string.IsNullOrEmpty(closureReason))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    string.Format(CultureInfo.InvariantCulture,
                        ErrorDateClosed,
                        closureReason));
            }

            // verify the requested date is not before the first available or 7 days after
            if (viewModel.RequestedDate.Value.Date < firstAvailable.Date)
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
            else if (viewModel.RequestedDate.Value.Date > firstAvailable.Date.AddDays(7))
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

            // if the selected date is today then ensure it's not before the first available time
            if (viewModel.RequestedDate.Value.Date == DateTime.Now.Date
                && viewModel.RequestedTime.Value.TimeOfDay < firstAvailable.TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    string.Format(CultureInfo.CurrentCulture,
                        ErrorEarliestTime,
                        firstAvailable.ToShortTimeString()));
                viewModel.RequestedTime = firstAvailable.ToLocalTime();
            }
            else
            {
                var startHour = await _siteSettingService.GetSettingDoubleAsync(
                    Models.Keys.SiteSetting.Scheduling.StartHour);
                var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                    Models.Keys.SiteSetting.Scheduling.AvailableHours);

                // if the selected date is not today, ensure times are within the allowed range
                if (viewModel.RequestedTime.Value.TimeOfDay
                    < firstAvailable.Date.AddHours(startHour).TimeOfDay)
                {
                    DateTime earliestTime;
                    if (firstAvailable.Date == viewModel.RequestedDate.Value.Date)
                    {
                        earliestTime = firstAvailable;
                    }
                    else
                    {
                        earliestTime = firstAvailable.Date.AddHours(startHour);
                    }

                    ModelState.AddModelError(nameof(viewModel.RequestedTime),
                        string.Format(CultureInfo.CurrentCulture,
                            ErrorEarliestTime,
                            earliestTime.ToShortTimeString()));
                    viewModel.RequestedTime = earliestTime.ToLocalTime();
                }
                else if (viewModel.RequestedTime.Value.TimeOfDay
                    > firstAvailable.Date.AddHours(startHour + availableHours).TimeOfDay)
                {
                    DateTime latestTime = firstAvailable.Date.AddHours(startHour + availableHours);

                    ModelState.AddModelError(nameof(viewModel.RequestedTime),
                        string.Format(CultureInfo.InvariantCulture,
                            ErrorTimeBefore,
                            latestTime.ToShortTimeString()));
                    viewModel.RequestedTime = latestTime.ToLocalTime();
                }
            }

            var requestDateTime = viewModel.RequestedDate.Value.Date
                    + viewModel.RequestedTime.Value.TimeOfDay;

            if (ModelState.IsValid)
            {
                // store stuff in tempdata
                TempData[TempDataDateTime] = requestDateTime;

                if (await _scheduleService.IsRequestOverLimitAsync(requestDateTime))
                {
                    // redirect to time form if timeslot is full
                    return RedirectToAction(nameof(ScheduleTimes));
                }
                else
                {
                    // redirect to second form
                    return RedirectToAction(nameof(ScheduleDetails));
                }
            }
            else
            {
                return await DisplayScheduleTimeFormAsync(viewModel,
                    viewModel.ScheduleRequestTime); ;
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

            var scheduleRequestSubjectId = TempData.Peek(TempDataSubjectId) as int?;
            var requestedTime = TempData.Peek(TempDataDateTime) as DateTime?;

            if (scheduleRequestSubjectId != null && requestedTime != null)
            {
                viewModel.ScheduleRequest.ScheduleRequestSubjectId = (int)scheduleRequestSubjectId;
                viewModel.ScheduleRequest.RequestedTime = (DateTime)requestedTime;
            }
            else
            {
                _logger.LogError("TempData items missing in SubmitScheduleDetails");
                return await DisplayScheduleFormAsync(null, SessionTimeout);
            }

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

                viewModel.ShowEmailMessage = selectedSubject.RequireEmail;

                return View("Scheduled", viewModel);
            }
            else
            {
                return await DisplayScheduleDetailsFormAsync(viewModel,
                    (int)scheduleRequestSubjectId,
                    (DateTime)requestedTime);
            }
        }

        private async Task<IActionResult> DisplayScheduleFormAsync(ScheduleViewModel viewModel,
            string warningMessage)
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

            scheduleViewModel.WarningText = warningMessage;

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

            try
            {
                var firstAvailable = await FirstAvailable(DateTime.Now);

                if (scheduleViewModel.RequestedDate == DateTime.MinValue)
                {
                    scheduleViewModel.RequestedDate = firstAvailable.Date;
                }

                if (scheduleViewModel.RequestedTime == DateTime.MinValue)
                {
                    scheduleViewModel.RequestedTime = firstAvailable.ToLocalTime();
                }
            }
            catch (OcudaException ex)
            {
                scheduleViewModel.WarningText = ex.Message;
            }

            return View("Schedule", scheduleViewModel);
        }

        private async Task<IActionResult>
            DisplayScheduleTimeFormAsync(ScheduleTimesViewModel viewModel,
            DateTime requestedTime)
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

            var scheduleViewModel = viewModel ?? new ScheduleTimesViewModel()
            {
                RequestedDate = requestedTime,
                RequestedTime = requestedTime
            };

            scheduleViewModel.ScheduleRequestTime = requestedTime;

            var startHour = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.StartHour);
            var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.AvailableHours);

            var firstAvailable = await FirstAvailable(DateTime.Now);
            var blockedDays = await GetBlockedDays();

            var daySuggestedTimes = await _scheduleService.GetDaySuggestedTimesAsync(
                requestedTime,
                startHour,
                availableHours,
                firstAvailable);

            var hourSuggestedTimes = await _scheduleService.GetHourSuggestedTimesAsync(
                requestedTime,
                firstAvailable,
                blockedDays);

            scheduleViewModel.SuggestedTimes = daySuggestedTimes
                .Concat(hourSuggestedTimes)
                .ToList();

            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            int segmentId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Scheduling.OverLimitSegment,
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

            return View(nameof(ScheduleTimes), scheduleViewModel);
        }

        private async Task<IActionResult>
            DisplayScheduleDetailsFormAsync(ScheduleDetailsViewModel viewModel,
            int selectedSubjectId,
            DateTime requestedTime)
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
                RequestedTime = requestedTime
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
