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
    public class ScheduleClaimRepository
        : OpsRepository<OpsContext, ScheduleClaim, int>, IScheduleClaimRepository
    {
        public ScheduleClaimRepository(Repository<OpsContext> repositoryFacade,
            ILogger<ScheduleClaimRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ScheduleClaim> AddSaveAsync(ScheduleClaim scheduleClaim)
        {
            var addedClaim = await DbSet.AddAsync(scheduleClaim);
            await _context.SaveChangesAsync();
            return addedClaim.Entity;
        }

        public async Task<IEnumerable<ScheduleClaim>> GetClaimsAsync(int[] scheduleRequestIds)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.User)
                .Where(_ => scheduleRequestIds.Contains(_.ScheduleRequestId))
                .ToListAsync();
        }

        public async Task<IEnumerable<ScheduleClaim>> GetClaimsForUserAsync(int userId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.User)
                .Where(_ => _.UserId == userId && !_.IsComplete)
                .ToListAsync();
        }
    }
}
