using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class EmailSetupTextRepository
        : GenericRepository<OpsContext, EmailSetupText>, IEmailSetupTextRepository
    {
        public EmailSetupTextRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<EmailSetupTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<EmailSetupText> GetByIdLanguageAsync(int emailSetupId,
            string languageName)
        {
            return await DbSet
                .Include(_ => _.EmailSetup)
                .Where(_ => _.EmailSetupId == emailSetupId
                    && _.PromenadeLanguageName == languageName)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
    }
}
