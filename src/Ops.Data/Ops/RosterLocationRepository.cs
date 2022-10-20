using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterLocationRepository : OpsRepository<OpsContext, RosterLocation, int>,
        IRosterLocationRepository
    {
        public RosterLocationRepository(Repository<OpsContext> repositoryFacade,
            ILogger<RosterLocationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<RosterLocation>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }
    }
}