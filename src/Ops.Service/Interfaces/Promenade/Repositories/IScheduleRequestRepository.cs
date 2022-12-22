using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduleRequestRepository : IGenericRepository<ScheduleRequest>
    {
        public Task<CollectionWithCount<ScheduleRequest>>
            GetPaginatedAsync(ScheduleRequestFilter filter);

        public Task<ICollection<ScheduleRequest>> GetPendingNotificationsAsync();

        public Task<ScheduleRequest> GetRequestAsync(int requestId);

        public Task<IEnumerable<ScheduleRequest>> GetRequestsAsync(DateTime requestedDate);

        public Task<IEnumerable<ScheduleRequest>> GetUnclaimedRequestsAsync();
    }
}