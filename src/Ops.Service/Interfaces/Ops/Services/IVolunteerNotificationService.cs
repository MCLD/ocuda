using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IVolunteerNotificationService
    {
        Task<int> SendPendingNotificationsAsync();
    }
}