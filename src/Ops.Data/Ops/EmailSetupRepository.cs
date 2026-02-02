using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class EmailSetupRepository 
        : GenericRepository<OpsContext, EmailSetup>, IEmailSetupRepository
    {
        public EmailSetupRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<EmailSetupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<IEnumerable<EmailSetup>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Description)
                .ToListAsync();
        }
    }
}
