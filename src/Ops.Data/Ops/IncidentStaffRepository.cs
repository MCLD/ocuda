using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentStaffRepository
        : OpsRepository<OpsContext, IncidentStaff, int>, IIncidentStaffRepository
    {
        public IncidentStaffRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentStaffRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
