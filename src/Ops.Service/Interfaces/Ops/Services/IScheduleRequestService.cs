using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScheduleRequestService
    {
        Task<ICollection<ScheduleRequest>> GetPendingNotificationsAsync();
        Task<ScheduleRequest> GetRequestAsync(int requestId);
        Task<IEnumerable<ScheduleRequest>> GetRequestsAsync(DateTime requestedDate);
        Task<IEnumerable<ScheduleRequest>> GetUnclaimedRequestsAsync();
        Task SetNotificationSentAsync(ScheduleRequest request);
        Task SetFollowupSentAsync(ScheduleRequest request);
    }
}
