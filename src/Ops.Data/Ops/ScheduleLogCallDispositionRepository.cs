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
    public class ScheduleLogCallDispositionRepository
        : OpsRepository<OpsContext, ScheduleLogCallDisposition, int>,
        IScheduleLogCallDispositionRepository
    {
        public ScheduleLogCallDispositionRepository(Repository<OpsContext> repositoryFacade,
            ILogger<ScheduleLogCallDispositionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<IEnumerable<ScheduleLogCallDisposition>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Disposition)
                .ToListAsync();
        }
    }
}