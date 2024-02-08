﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Help;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Helpers;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public partial class HelpController : BaseController<HelpController>
    {
        private const string TempDataDateTime = "ScheduleDateTime";
        private const string TempDataSubjectId = "ScheduleSubjectId";
        /* Duplicate Interval in HelpController.cs, potentially should be site setting */
        private const double TimeBlockInterval = 0.5;

        private const string ViewModelEmail = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.Email);

        private const string ViewModelNotes = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.Notes);

        private const string ViewModelTelephoneId = nameof(ScheduleRequest)
            + "."
            + nameof(ScheduleRequest.ScheduleRequestTelephoneId);

        private static readonly TimeSpan QuantizeSpan = TimeSpan.FromMinutes(30);

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly LocationService _locationService;
        private readonly ScheduleService _scheduleService;
        private readonly SegmentService _segmentService;

        public HelpController(ServiceFacades.Controller<HelpController> context,
            IDateTimeProvider dateTimeProvider,
            LocationService locationService,
            ScheduleService scheduleService,
            SegmentService segmentService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(scheduleService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _dateTimeProvider = dateTimeProvider;
            _locationService = locationService;
            _scheduleService = scheduleService;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "Help"; } }

        [HttpGet("[action]")]
        public async Task<JsonResult> Availability()
        {
            return await Availability(string.Empty);
        }

        [HttpGet("[action]/{selectedDate}")]
        public async Task<JsonResult> Availability(string selectedDate)
        {
            var availableBlocks = DateTime.TryParse(selectedDate, out var date)
                ? await GetTimeBlocks(date)
                : await GetTimeBlocks();
            return Json(availableBlocks.Select(_ => new { value = _.Value, text = _.Text }));
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Schedule()
        {
            return await DisplayScheduleFormAsync(null, null);
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
                return await DisplayScheduleFormAsync(null,
                    _localizer[i18n.Keys.Promenade.ErrorSessionTimeout]);
            }

            return await DisplayScheduleDetailsFormAsync(null,
                (int)scheduleRequestSubjectId,
                (DateTime)requestedTime);
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
                return await DisplayScheduleFormAsync(null,
                    _localizer[i18n.Keys.Promenade.ErrorSessionTimeout]);
            }

            return await DisplayScheduleTimeFormAsync(null, (DateTime)requestedTime);
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
                firstAvailable = await FirstAvailable(_dateTimeProvider.Now);
            }
            catch (OcudaException)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var blockedDays = await GetBlockedDays();

            if (blockedDays.Contains(viewModel.RequestedDate.DayOfWeek))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorAvailableDaysItem,
                        new DayOfWeekHelper().ListDays(blockedDays)]);
            }

            // check for all-location closure on that day
            var closureReason = await _locationService
                .GetClosureInformationAsync(viewModel.RequestedDate);

            if (!string.IsNullOrEmpty(closureReason))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorDateClosedItem,
                        closureReason]);
            }

            // verify the requested date is not before the first available or 7 days after
            if (viewModel.RequestedDate.Date < firstAvailable.Date)
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorDateOnAfterItem,
                        firstAvailable.ToShortDateString()]);
                viewModel.RequestedDate = firstAvailable.Date;
            }
            else if (viewModel.RequestedDate.Date > firstAvailable.Date.AddDays(7))
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorFurthestDateItem,
                        firstAvailable.AddDays(7).ToShortDateString()]);
                viewModel.RequestedDate = firstAvailable.Date.AddDays(7);
            }

            // if the selected date is today then ensure it's not before the first available time
            if (viewModel.RequestedDate.Date == _dateTimeProvider.Now.Date
                && viewModel.RequestedTime.TimeOfDay < firstAvailable.TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    _localizer[i18n.Keys.Promenade.ErrorEarliestTimeItem,
                        firstAvailable.ToShortTimeString()]);
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
                        _localizer[i18n.Keys.Promenade.ErrorEarliestTimeItem,
                            earliestTime.ToShortTimeString()]);
                    viewModel.RequestedTime = earliestTime.ToLocalTime();
                }
                else if (viewModel.RequestedTime.TimeOfDay
                    > firstAvailable.Date.AddHours(startHour + availableHours).TimeOfDay)
                {
                    DateTime latestTime = firstAvailable.Date.AddHours(startHour + availableHours);

                    ModelState.AddModelError(nameof(viewModel.RequestedTime),
                        _localizer[i18n.Keys.Promenade.ErrorTimeBeforeItem,
                            latestTime.ToShortTimeString()]);
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
                return await DisplayScheduleFormAsync(null,
                    _localizer[i18n.Keys.Promenade.ErrorSessionTimeout]);
            }

            string phoneNumbers;
            if (!string.IsNullOrEmpty(viewModel.ScheduleRequestPhone))
            {
                phoneNumbers = DigitsOnlyRegex().Replace(viewModel.ScheduleRequestPhone, "");
                if (phoneNumbers.Length != 10)
                {
                    ModelState.AddModelError(nameof(viewModel.ScheduleRequestPhone),
                        _localizer[i18n.Keys.Promenade.ErrorTelephoneFormat]);
                }
            }

            var selectedSubject = subjects
                .SingleOrDefault(_ => _.Id == viewModel.ScheduleRequest.ScheduleRequestSubjectId);

            if (selectedSubject.RequireEmail
                && string.IsNullOrWhiteSpace(viewModel.ScheduleRequest.Email))
            {
                ModelState.AddModelError(ViewModelEmail,
                    _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                        _localizer[i18n.Keys.Promenade.PromptEmail]]);
            }

            if (selectedSubject.RequireComments
                && string.IsNullOrWhiteSpace(viewModel.ScheduleRequest.Notes))
            {
                ModelState.AddModelError(ViewModelNotes,
                    _localizer[i18n.Keys.Promenade.ScheduleRequiredNotes]);
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
                        viewModel.SegmentText.Text = FormatForDisplay(viewModel.SegmentText);
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
                    ModelState.AddModelError(nameof(viewModel.RequestedDate),
                        _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                            _localizer[i18n.Keys.Promenade.PromptRequestedDate]]);
                }
                if (!viewModel.RequestedTime.HasValue)
                {
                    ModelState.AddModelError(nameof(viewModel.RequestedDate),
                        _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                            _localizer[i18n.Keys.Promenade.PromptRequestedTime]]);
                }

                var requestedTime = TempData.Peek(TempDataDateTime) as DateTime?;

                if (requestedTime == null)
                {
                    _logger.LogError("TempData items missing in ScheduleTime");
                    return await DisplayScheduleFormAsync(null,
                        _localizer[i18n.Keys.Promenade.ErrorSessionTimeout]);
                }

                return await DisplayScheduleTimeFormAsync(null, (DateTime)requestedTime);
            }

            DateTime firstAvailable;
            try
            {
                firstAvailable = await FirstAvailable(_dateTimeProvider.Now);
            }
            catch (OcudaException)
            {
                return RedirectToAction(nameof(Schedule));
            }

            var blockedDays = await GetBlockedDays();

            if (blockedDays.Contains(viewModel.RequestedDate.Value.DayOfWeek))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorAvailableDaysItem,
                        new DayOfWeekHelper().ListDays(blockedDays)]);
            }

            // check for all-location closure on that day
            var closureReason = await _locationService
                .GetClosureInformationAsync(viewModel.RequestedDate.Value);

            if (!string.IsNullOrEmpty(closureReason))
            {
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorDateClosedItem,
                        closureReason]);
            }

            // verify the requested date is not before the first available or 7 days after
            if (viewModel.RequestedDate.Value.Date < firstAvailable.Date)
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorDateOnAfterItem,
                        firstAvailable.ToShortDateString()]);
                viewModel.RequestedDate = firstAvailable.Date;
            }
            else if (viewModel.RequestedDate.Value.Date > firstAvailable.Date.AddDays(7))
            {
                if (ModelState.ContainsKey(nameof(viewModel.RequestedDate)))
                {
                    ModelState.Remove(nameof(viewModel.RequestedDate));
                }
                ModelState.AddModelError(nameof(viewModel.RequestedDate),
                    _localizer[i18n.Keys.Promenade.ErrorFurthestDateItem,
                        firstAvailable.AddDays(7).ToShortDateString()]);
                viewModel.RequestedDate = firstAvailable.Date.AddDays(7);
            }

            // if the selected date is today then ensure it's not before the first available time
            if (viewModel.RequestedDate.Value.Date == _dateTimeProvider.Now.Date
                && viewModel.RequestedTime.Value.TimeOfDay < firstAvailable.TimeOfDay)
            {
                ModelState.AddModelError(nameof(viewModel.RequestedTime),
                    _localizer[i18n.Keys.Promenade.ErrorEarliestTimeItem,
                        firstAvailable.ToShortTimeString()]);
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
                        _localizer[i18n.Keys.Promenade.ErrorEarliestTimeItem,
                            earliestTime.ToShortTimeString()]);
                    viewModel.RequestedTime = earliestTime.ToLocalTime();
                }
                else if (viewModel.RequestedTime.Value.TimeOfDay
                    > firstAvailable.Date.AddHours(startHour + availableHours).TimeOfDay)
                {
                    DateTime latestTime = firstAvailable.Date.AddHours(startHour + availableHours);

                    ModelState.AddModelError(nameof(viewModel.RequestedTime),
                        _localizer[i18n.Keys.Promenade.ErrorTimeBeforeItem,
                            latestTime.ToShortTimeString()]);
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
                    viewModel.ScheduleRequestTime);
            }
        }

        [GeneratedRegex("[^0-9.]")]
        private static partial Regex DigitsOnlyRegex();

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
                scheduleViewModel.EmailRequiredMessage
                    = _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                        _localizer[i18n.Keys.Promenade.PromptEmail]];
            }

            if (subject.RequireComments)
            {
                scheduleViewModel.NotesRequiredMessage
                    = _localizer[i18n.Keys.Promenade.ScheduleRequiredNotes];
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
                        = FormatForDisplay(scheduleViewModel.SegmentText);
                }
            }

            scheduleViewModel.DisplaySubject = subject.SubjectText;

            scheduleViewModel.ScheduleRequest = new ScheduleRequest
            {
                RequestedTime = requestedTime
            };

            return View("ScheduleDetails", scheduleViewModel);
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
                if (TempData.TryGetValue(TempDataDateTime, out object tempDateTime))
                {
                    var dateTime = tempDateTime as DateTime?;
                    if (dateTime != null)
                    {
                        scheduleViewModel.RequestedDate = dateTime.Value.Date;
                        scheduleViewModel.RequestedTime = dateTime.Value;
                    }
                }

                if (TempData.TryGetValue(TempDataSubjectId, out object subjectId))
                {
                    scheduleViewModel.SubjectId = (int)subjectId;
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
                        = FormatForDisplay(scheduleViewModel.SegmentText);
                }
            }

            scheduleViewModel.Subjects = subjects.Select(_ => new SelectListItem
            {
                Text = _.SubjectText,
                Value = _.Id.ToString(CultureInfo.InvariantCulture)
            });

            try
            {
                var firstAvailable = await FirstAvailable(_dateTimeProvider.Now);

                scheduleViewModel.FirstAvailable = firstAvailable;
                scheduleViewModel.LastAvailableDate = firstAvailable.AddDays(7);

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

            scheduleViewModel.TimeBlocks = await GetTimeBlocks(scheduleViewModel.RequestedTime);

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
                RequestedTime = _dateTimeProvider.Now.Date.Add(requestedTime.TimeOfDay)
            };

            scheduleViewModel.ScheduleRequestTime = requestedTime;

            var startHour = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.StartHour);
            var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.AvailableHours);

            var firstAvailable = await FirstAvailable(_dateTimeProvider.Now);
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

            if (segmentId != default && segmentId > 0)
            {
                scheduleViewModel.SegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(segmentId, forceReload);

                if (!string.IsNullOrEmpty(scheduleViewModel.SegmentText?.Text))
                {
                    scheduleViewModel.SegmentText.Text
                        = FormatForDisplay(scheduleViewModel.SegmentText);
                }
            }

            scheduleViewModel.TimeBlocks = await GetTimeBlocks(requestedTime);

            return View(nameof(ScheduleTimes), scheduleViewModel);
        }

        private async Task<DateTime> FirstAvailable(DateTime date)
        {
            var blockedDays = await GetBlockedDays();

            if (blockedDays.Count >= Enum.GetNames(typeof(DayOfWeek)).Length)
            {
                _logger.LogCritical("No available days configured for scheduling.");
                throw new OcudaException(_localizer[i18n.Keys.Promenade.ErrorNoTimes]);
            }

            var startHour = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.StartHour);
            var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.AvailableHours);
            var bufferHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.BufferHours);

            var firstAvailable = date.RoundUp(QuantizeSpan).AddHours(bufferHours);

            var isOverLimit = await _scheduleService.IsRequestOverLimitAsync(firstAvailable);

            while (isOverLimit && firstAvailable.Hour <= startHour + availableHours)
            {
                firstAvailable = firstAvailable.AddHours(TimeBlockInterval);
                isOverLimit = await _scheduleService.IsRequestOverLimitAsync(firstAvailable);
            }

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
                if (Enum.TryParse(day, true, out DayOfWeek result))
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

        private async Task<IEnumerable<SelectListItem>> GetTimeBlocks()
        {
            return await GetTimeBlocks(null);
        }

        private async Task<IEnumerable<SelectListItem>> GetTimeBlocks(DateTime? requestedDate)
        {
            var startHour = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.StartHour);
            var availableHours = await _siteSettingService.GetSettingDoubleAsync(
                Models.Keys.SiteSetting.Scheduling.AvailableHours);

            if (!requestedDate.HasValue
                || requestedDate.Value.Date <= _dateTimeProvider.Now)
            {
                requestedDate = await FirstAvailable(_dateTimeProvider.Now);
                availableHours -= requestedDate.Value.Hour - startHour;
                startHour = requestedDate.Value.Hour;
            }

            return await _scheduleService
                .GetAvailableTimeBlocks(startHour, availableHours, requestedDate.Value);
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