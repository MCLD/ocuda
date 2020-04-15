using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScheduleService
    {
        public Task AddLog(ScheduleLog log);
        public Task<IEnumerable<ScheduleClaim>> GetClaimsAsync(int[] scheduleRequestIds);
        public Task<IEnumerable<ScheduleClaim>> GetCurrentUserClaimsAsync();
        public Task<IEnumerable<ScheduleLog>> GetLogAsync(int scheduleRequestId);
        public Task<IEnumerable<ScheduleLogCallDisposition>> GetCallDispositionsAsync();
        public Task<ScheduleClaim> AddAsync(int scheduleRequestId);
    }
}
