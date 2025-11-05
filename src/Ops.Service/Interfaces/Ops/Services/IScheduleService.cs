using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScheduleService
    {
        public Task AddLogAsync(ScheduleLog log, bool setUnderway);
        public Task<IEnumerable<ScheduleClaim>> GetClaimsAsync(int[] scheduleRequestIds);
        public Task<IEnumerable<ScheduleClaim>> GetCurrentUserClaimsAsync();
        public Task<IEnumerable<ScheduleLog>> GetLogAsync(int scheduleRequestId);
        public Task<IEnumerable<ScheduleLogCallDisposition>> GetCallDispositionsAsync();
        public Task<ScheduleClaim> ClaimAsync(int scheduleRequestId);
        public Task UnclaimAsync(int scheduleRequestId);
        public Task CancelAsync(ScheduleLog cancelLog);
    }
}
