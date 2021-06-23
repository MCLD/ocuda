using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class DigitalDisplaySetRepository
        : OpsRepository<OpsContext, DigitalDisplaySet, int>, IDigitalDisplaySetRepository
    {
        public DigitalDisplaySetRepository(Repository<OpsContext> repositoryFacade,
            ILogger<DigitalDisplaySetRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<DigitalDisplaySet>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().OrderBy(_ => _.Name).ToListAsync();
        }

        public async Task<DigitalDisplaySet> GetByName(string setName)
        {
            return await DbSet.AsNoTracking().SingleOrDefaultAsync(_ => _.Name == setName);
        }
    }
}