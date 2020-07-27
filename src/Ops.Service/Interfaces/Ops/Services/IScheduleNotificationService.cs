using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScheduleNotificationService
    {
        Task<int> SendPendingNotificationsAsync();
        Task SendFollowupAsync(ScheduleRequest request);
        Task<Ocuda.Ops.Models.Entities.EmailRecord> SendCancellationAsync(ScheduleRequest request);
    }
}
