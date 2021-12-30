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
    }
}
