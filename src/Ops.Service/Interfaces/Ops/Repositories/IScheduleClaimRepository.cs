using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IScheduleClaimRepository : IOpsRepository<ScheduleClaim, int>
    {
        public Task<IEnumerable<ScheduleClaim>> GetClaimsAsync(int[] scheduleRequestIds);
        public Task<IEnumerable<ScheduleClaim>> GetClaimsForUserAsync(int userId);
        public Task<ScheduleClaim> AddSaveAsync(ScheduleClaim scheduleClaim);
    }
}
