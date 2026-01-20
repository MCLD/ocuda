using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Email;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IEmailService
    {
        Task<Details> GetDetailsAsync(int emailSetupId, 
            string languageName,
            IDictionary<string, string> tags, 
            string overrideText = null);
        Task<Dictionary<int, string>> GetEmailSetupsAsync();
        Task<EmailSetupText> GetSetupTextByLanguageAsync(int emailSetupId, string languageName);
        Task<EmailRecord> SendAsync(Details emailDetails);
    }
}