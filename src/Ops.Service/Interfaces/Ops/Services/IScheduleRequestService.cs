using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScheduleRequestService
    {
        Task CancelAsync(ScheduleRequest request);

        Task<ICollection<ScheduleRequest>> GetPendingNotificationsAsync();

        Task<ScheduleRequest> GetRequestAsync(int requestId);

        Task<CollectionWithCount<ScheduleRequest>> GetRequestsAsync(ScheduleRequestFilter filter);

        Task<ScheduleRequest> SetClaimedAsync(int scheduleRequestId);

        Task SetFollowupSentAsync(ScheduleRequest request);

        Task SetNotificationSentAsync(ScheduleRequest request);

        Task SetUnderwayAsync(int scheduleRequestId);

        Task UnclaimAsync(int scheduleRequestId);
    }
}