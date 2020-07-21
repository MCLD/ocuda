using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScheduleNotificationService
    {
        Task SendPendingNotificationsAsync();
        Task SendFollowupAsync(ScheduleRequest request);
    }
}
