using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IScheduleRequestLimitRepository : IGenericRepository<ScheduleRequestLimit>
    {
        Task<ICollection<ScheduleRequestLimit>> GetLimitsForDayAsync(DayOfWeek day);
        Task<ICollection<ScheduleRequestLimit>> GetLimitsForHourAsync(int hour);
        Task<int?> GetTimeSlotLimitAsync(DateTime requestTime);
    }
}
