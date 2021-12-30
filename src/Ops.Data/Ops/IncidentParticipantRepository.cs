using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentParticipantRepository 
        : OpsRepository<OpsContext, IncidentParticipant, int>, IIncidentParticipantRepository
    {
        public IncidentParticipantRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentParticipantRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
