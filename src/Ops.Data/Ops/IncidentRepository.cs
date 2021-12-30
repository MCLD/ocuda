using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentRepository : OpsRepository<OpsContext, Incident, int>, IIncidentRepository
    {
        public IncidentRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
