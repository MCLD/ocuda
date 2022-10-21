using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterDivisionRepository : OpsRepository<OpsContext, RosterDivision, int>,
        IRosterDivisionRepository
    {
        public RosterDivisionRepository(Repository<OpsContext> repositoryFacade,
            ILogger<RosterDivisionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<RosterDivision>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }
    }
}