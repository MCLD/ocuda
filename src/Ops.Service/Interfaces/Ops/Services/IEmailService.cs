using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Email;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IEmailService
    {
        Task<EmailSetupText> GetEmailSetupAsync(int emailSetupId, string languageName);
        Task<EmailTemplateText> GetEmailTemplateAsync(int emailTemplateId, string languageName);
        Task<Record> SendAsync(Details emailDetails);
    }
}
