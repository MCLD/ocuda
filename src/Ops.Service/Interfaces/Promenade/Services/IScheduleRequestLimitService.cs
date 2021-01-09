using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IScheduleRequestLimitService
    {
        Task<ICollection<ScheduleRequestLimit>> GetLimitsForDayAsync(DayOfWeek day);
        Task SetScheduleDayLimtsAsnyc(DayOfWeek day, IEnumerable<ScheduleRequestLimit> dayLimits);
    }
}
