using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IEmailService
    {
        Task SendToUserAsync(int userId, string subject, string body, string htmlBody = null);
        Task SendToAddressAsync(string emailAddress, string subject, string body,
            string htmlBody = null);
    }
}
