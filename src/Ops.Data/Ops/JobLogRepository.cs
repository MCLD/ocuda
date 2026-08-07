using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class JobLogRepository(
        Repository<OpsContext> repositoryFacade,
        ILogger<JobLogRepository> logger)
        : OpsRepository<OpsContext, JobLog, int>(repositoryFacade, logger), IJobLogRepository
    {
    }
}
