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
    public class IncidentRelationshipRepository
        : OpsRepository<OpsContext, IncidentRelationship, int>, IIncidentRelationshipRepository
    {
        public IncidentRelationshipRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentRelationshipRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<IncidentRelationship>> GetByIncidentIdAsync(int incidentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IncidentId == incidentId || _.RelatedIncidentId == incidentId)
                .ToListAsync();
        }
    }
}
