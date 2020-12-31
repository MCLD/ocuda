using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduleRequestLimitRepository : IGenericRepository<ScheduleRequestLimit>
    {
        Task<ICollection<ScheduleRequestLimit>> GetLimitsForDayAsync(DayOfWeek day);
    }
}
