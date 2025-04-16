using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IScheduleLogRepository : IOpsRepository<ScheduleLog, int>
    {
        public Task<IEnumerable<ScheduleLog>> GetByScheduleRequestIdAsync(int scheduleRequestId);
    }
}