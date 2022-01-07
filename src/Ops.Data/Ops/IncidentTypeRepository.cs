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
    public class IncidentTypeRepository
        : OpsRepository<OpsContext, IncidentType, int>, IIncidentTypeRepository
    {
        public IncidentTypeRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentTypeRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<IncidentType>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().OrderBy(_ => _.Description).ToListAsync();
        }
    }
}
