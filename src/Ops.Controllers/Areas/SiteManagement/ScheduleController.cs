using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Schedule;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{

    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class ScheduleController : BaseController<ScheduleController>
    {
        private const int HoursInADay = 24;

        private readonly IScheduleRequestLimitService _scheduleRequestLimitService;
        private readonly ISiteSettingPromService _siteSettingPromService;

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "Schedule"; } }

        public ScheduleController(ServiceFacades.Controller<ScheduleController> context,
            IScheduleRequestLimitService scheduleRequestLimitService,
            ISiteSettingPromService siteSettingPromService)
            : base(context)
        {
            _scheduleRequestLimitService = scheduleRequestLimitService
                ?? throw new ArgumentNullException(nameof(scheduleRequestLimitService));
            _siteSettingPromService = siteSettingPromService
                ?? throw new ArgumentNullException(nameof(siteSettingPromService));
        }

        [Route("")]
        [Route("{day}")]
        [RestoreModelState]
        public async Task<IActionResult> Limits(string day)
        {
            var unavailableDays = (await _siteSettingPromService
                .GetSettingStringAsync(Promenade.Models.Keys.SiteSetting.Scheduling.UnavailableDays))
                ?.Split(',')
                .Select(_ => _?.Trim())
                .Distinct();

            var blockedDays = new List<DayOfWeek>();

            foreach (var dayOfWeek in unavailableDays)
            {
                if (Enum.TryParse<DayOfWeek>(dayOfWeek, true, out DayOfWeek result))
                {
                    blockedDays.Add(result);
                }
                else
                {
                    _logger.LogWarning("Invalid blocked day for scheduling: {day}", dayOfWeek);
                }
            }

            var availableDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .Except(blockedDays);

            if (!availableDays.Any())
            {
                ShowAlertWarning("Scheduling has no available days");
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            if (!Enum.TryParse<DayOfWeek>(day, true, out DayOfWeek selectedDay))
            {
                selectedDay = availableDays.First();
            }

            var limits = await _scheduleRequestLimitService.GetLimitsForDayAsync(selectedDay);

            var startHour = await _siteSettingPromService.GetSettingDoubleAsync(
                Promenade.Models.Keys.SiteSetting.Scheduling.StartHour);
            var availableHours = await _siteSettingPromService.GetSettingDoubleAsync(
                Promenade.Models.Keys.SiteSetting.Scheduling.AvailableHours);

            var dayLimits = new Dictionary<int, int?>();

            for (int hour = (int)startHour; hour < Math.Ceiling(startHour + availableHours); hour++)
            {
                var dayHour = hour % HoursInADay;

                var limit = limits
                    .Where(_ => _.Hour == dayHour)
                    .Select(_ => (int?)_.Limit)
                    .SingleOrDefault();

                dayLimits.Add(dayHour, limit);

                if (dayLimits.Count >= HoursInADay)
                {
                    break;
                }
            }

            dayLimits = dayLimits.OrderBy(_ => _.Key).ToDictionary(_ => _.Key, _ => _.Value);

            var viewModel = new LimitsViewModel
            {
                AvailableDays = availableDays,
                SelectedDay = selectedDay,
                DayLimits = dayLimits
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("")]
        [Route("{day}")]
        [SaveModelState]
        public async Task<IActionResult> Limits(LimitsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dayLimits = model.DayLimits
                    .Where(_ => _.Value >= 0)
                    .Select(_ => new ScheduleRequestLimit
                    {
                        DayOfWeek = model.SelectedDay,
                        Hour = _.Key,
                        Limit = _.Value.Value
                    });

                await _scheduleRequestLimitService.SetScheduleDayLimtsAsnyc(model.SelectedDay,
                    dayLimits);

                ShowAlertSuccess("Limits saved!");
            }

            return RedirectToAction(nameof(Limits));
        }
    }
}
