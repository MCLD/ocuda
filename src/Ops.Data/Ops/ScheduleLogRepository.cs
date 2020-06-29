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
    public class ScheduleLogRepository
           : OpsRepository<OpsContext, ScheduleLog, int>,
           IScheduleLogRepository
    {
        public ScheduleLogRepository(Repository<OpsContext> repositoryFacade,
            ILogger<ScheduleLogRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<IEnumerable<ScheduleLog>> GetByScheduleRequestIdAsync(int scheduleRequestId)
        {
            return await DbSet
                .Include(_ => _.ScheduleLogCallDisposition)
                .Where(_ => _.ScheduleRequestId == scheduleRequestId)
                .OrderByDescending(_ => _.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
