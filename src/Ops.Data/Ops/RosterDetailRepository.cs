using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterDetailRepository
        : OpsRepository<OpsContext, RosterDetail, int>, IRosterDetailRepository
    {
        public RosterDetailRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<RosterDetailRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddRangeAsync(IEnumerable<RosterDetail> rosterDetails)
        {
            await DbSet.AddRangeAsync(rosterDetails);
        }

        public async Task<RosterDetail> GetAsync(int rosterId, string email)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.RosterHeaderId == rosterId && _.EmailAddress == email)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<RosterDetail>> GetByHeaderIdAsync(int rosterHeaderId)
        {
            return await DbSet
                 .AsNoTracking()
                 .Where(_ => _.RosterHeaderId == rosterHeaderId)
                 .ToListAsync();
        }

        public async Task<int> GetCountAsync(int rosterHeaderId)
        {
            return await DbSet.AsNoTracking().CountAsync(_ => _.RosterHeaderId == rosterHeaderId);
        }
    }
}