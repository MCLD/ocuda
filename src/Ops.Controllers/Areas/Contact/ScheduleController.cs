﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Contact;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Controllers.Areas.Contact
{
    [Area("Contact")]
    [Route("[area]/[controller]")]
    public class ScheduleController : BaseController<ScheduleController>
    {
        private readonly IScheduleService _scheduleService;
        private readonly IScheduleRequestService _scheduleRequestService;

        public ScheduleController(ServiceFacades.Controller<ScheduleController> context,
            IScheduleService scheduleService,
            IScheduleRequestService scheduleRequestService)
            : base(context)
        {
            _scheduleService = scheduleService
                ?? throw new ArgumentNullException(nameof(scheduleService));
            _scheduleRequestService = scheduleRequestService
                ?? throw new ArgumentNullException(nameof(scheduleRequestService));
        }

        [Route("")]
        [Route("fordate/{requestedDate}")]
        public async Task<IActionResult> Index(string requestedDate)
        {
            DateTime date = DateTime.MinValue;

            if (!string.IsNullOrEmpty(requestedDate)
               && !DateTime.TryParse(requestedDate, out date))
            {
                AlertInfo = $"Not able to display date: {requestedDate}";
            }

            var requests = date == DateTime.MinValue
                ? await _scheduleRequestService.GetUnclaimedRequestsAsync()
                : await _scheduleRequestService.GetRequestsAsync(date);

            var claims = await _scheduleService
                .GetClaimsAsync(requests.Select(_ => _.Id).ToArray());

            return View(new ScheduleIndexViewModel
            {
                ViewDescription = date == DateTime.MinValue
                    ? "Unclaimed"
                    : date.ToShortDateString(),
                RequestedDate = date == DateTime.MinValue ? DateTime.Now : date,
                Requests = requests,
                Claims = claims
            });
        }

        [Route("[action]/{requestId}")]
        public async Task<IActionResult> Details(int requestId)
        {
            var viewModel = new ScheduleDetailViewModel
            {
                ScheduleRequest = await _scheduleRequestService.GetRequestAsync(requestId)
            };

            if (viewModel.ScheduleRequest == null)
            {
                AlertDanger = $"Could not find request {requestId}.";
            }
            else
            {
                var claims = await _scheduleService.GetClaimsAsync(
                    new int[] { viewModel.ScheduleRequest.Id });
                viewModel.ScheduleClaim = claims.FirstOrDefault();
                viewModel.ScheduleLogs
                    = await _scheduleService.GetLogAsync(viewModel.ScheduleRequest.Id);
            }

            if (viewModel.ScheduleClaim?.UserId == CurrentUserId)
            {
                viewModel.IsClaimedByCurrentUser = true;
                var dispositions = await _scheduleService.GetCallDispositionsAsync();
                viewModel.CallDispositions = dispositions.Select(_ => new SelectListItem
                {
                    Text = _.Disposition,
                    Value = _.Id.ToString(CultureInfo.InvariantCulture)
                }).Prepend(new SelectListItem());
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Claim(int requestId)
        {
            await _scheduleService.AddAsync(requestId);

            return RedirectToAction(nameof(Details), new { requestId });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddLog(ScheduleDetailViewModel viewModel)
        {
            var addLog = viewModel?.AddLog;

            if (addLog.ScheduleLogCallDispositionId != null
                || addLog.DurationMinutes != null
                || addLog.Notes != null
                || addLog.IsComplete)
            {
                await _scheduleService.AddLog(addLog);
            }

            return RedirectToAction(nameof(Details),
                new { requestId = addLog.ScheduleRequestId });
        }
    }
}