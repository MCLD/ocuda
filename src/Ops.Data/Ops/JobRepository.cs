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
    public class JobRepository(
        Repository<OpsContext> repositoryFacade,
        ILogger<JobRepository> logger)
        : OpsRepository<OpsContext, Job, int>(repositoryFacade, logger), IJobRepository
    {
        public async Task<Job> GetLastCompletedFinishedTime(JobType jobType)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FinishedAt.HasValue)
                .OrderByDescending(_ => _.FinishedAt)
                .Take(1)
                .SingleOrDefaultAsync();
        }

        public async Task<Job> GetLastCreatedAsync(JobType jobType)
        {
            return await DbSet
                .AsNoTracking()
                .OrderByDescending(_ => _.CreatedAt)
                .Take(1)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Job>> GetPendingAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.StartedAt.HasValue)
                .OrderBy(_ => _.CreatedAt)
                .ToArrayAsync();
        }
    }
}
