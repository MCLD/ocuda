using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class EmailTemplateTextRepository
        : GenericRepository<OpsContext, EmailTemplateText>, IEmailTemplateTextRepository
    {
        public EmailTemplateTextRepository(Repository<OpsContext> repositoryFacade,
            ILogger<EmailTemplateTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<EmailTemplateText> GetByIdLanguageAsync(int emailTemplateId,
            string languageName)
        {
            return await DbSet
                .Where(_ => _.EmailTemplateId == emailTemplateId
                    && _.PromenadeLanguageName == languageName)
                .Select(_ => new EmailTemplateText
                {
                    EmailTemplateId = _.EmailTemplateId,
                    TemplateHtml = _.TemplateHtml,
                    TemplateText = _.TemplateText
                })
                .AsNoTracking()
                .SingleAsync();
        }
    }
}
