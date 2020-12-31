using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IScheduleRequestRepository : IGenericRepository<ScheduleRequest>
    {
        public Task<ScheduleRequest> AddSaveAsync(ScheduleRequest scheduleRequest);
        Task<ICollection<DataWithCount<int>>> GetDayRequestCountsAsync(DateTime requesteTime,
            DateTime firstAvailable);
        Task<ICollection<DataWithCount<DateTime>>> GetHourRequestCountsAsync(DateTime requesteTime, 
            DateTime firstAvailable);
        Task<int> GetTimeSlotCountAsync(DateTime requestTime);
    }
}
