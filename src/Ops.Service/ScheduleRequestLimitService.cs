using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service
{
    public class ScheduleRequestLimitService
        : BaseService<ScheduleRequestLimitService>, IScheduleRequestLimitService
    {
        private readonly IScheduleRequestLimitRepository _scheduleRequestLimitRepository;

        public ScheduleRequestLimitService(ILogger<ScheduleRequestLimitService> logger,
            IHttpContextAccessor httpContextAccessor,
            IScheduleRequestLimitRepository scheduleRequestLimitRepository)
            : base(logger, httpContextAccessor)
        {
            _scheduleRequestLimitRepository = scheduleRequestLimitRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestLimitRepository));
        }

        public async Task<ICollection<ScheduleRequestLimit>> GetLimitsForDayAsync(DayOfWeek day)
        {
            return await _scheduleRequestLimitRepository.GetLimitsForDayAsync(day);
        }

        public async Task SetScheduleDayLimtsAsnyc(DayOfWeek day,
            IEnumerable<ScheduleRequestLimit> dayLimits)
        {
            var currentLimits = await _scheduleRequestLimitRepository.GetLimitsForDayAsync(day);

            var limitsToAdd = dayLimits
                .Where(_ => !currentLimits.Select(l => l.Hour).Contains(_.Hour)).ToList();

            var limitsToRemove = currentLimits
                .Where(_ => !dayLimits.Select(l => l.Hour).Contains(_.Hour)).ToList();

            var limitsToUpdate = currentLimits.Except(limitsToRemove).ToList();
            foreach (var limit in limitsToUpdate)
            {
                var newLimit = dayLimits.Single(_ => _.Hour == limit.Hour);
                limit.Limit = newLimit.Limit;
            }

            await _scheduleRequestLimitRepository.AddRangeAsync(limitsToAdd);
            _scheduleRequestLimitRepository.RemoveRange(limitsToRemove);
            _scheduleRequestLimitRepository.UpdateRange(limitsToUpdate);
            await _scheduleRequestLimitRepository.SaveAsync();
        }
    }
}
