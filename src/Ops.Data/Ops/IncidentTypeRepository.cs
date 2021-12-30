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
    }
}
