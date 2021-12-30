using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentFollowupRepository
        : OpsRepository<OpsContext, IncidentFollowup, int>, IIncidentFollowupRepository
    {
        public IncidentFollowupRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentFollowupRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
