using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class JobConfigurationRepository(
        Repository<OpsContext> repositoryFacade,
        ILogger<JobConfigurationRepository> logger)
        : OpsRepository<OpsContext, JobConfiguration, int>(repositoryFacade, logger),
        IJobConfigurationRepository
    {
        public async Task<IEnumerable<JobType>> GetAllTypes()
        {
            return await DbSet
                .AsNoTracking()
                .Select(_ => _.Id)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<JobConfiguration>> GetAutomaticallyScheduleAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.AutomaticallySchedule == true)
                .ToListAsync();
        }
    }
}
