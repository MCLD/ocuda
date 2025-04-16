using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IEmailTemplateTextRepository : IGenericRepository<EmailTemplateText>
    {
        public Task<EmailTemplateText> GetByIdLanguageAsync(int emailTemplateId,
            string languageName);
    }
}