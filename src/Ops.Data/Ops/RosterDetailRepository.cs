using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterDetailRepository
        : GenericRepository<Models.RosterDetail, int>, IRosterDetailRepository
    {
        public RosterDetailRepository(OpsContext context, ILogger<RosterDetailRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task AddRangeAsync(IEnumerable<RosterDetail> rosterDetails) {
            await DbSet.AddRangeAsync(rosterDetails);
        }

        public async Task<RosterDetail> GetAsync(int rosterId, string email)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.RosterHeaderId == rosterId && _.EmailAddress == email)
                .FirstOrDefaultAsync();
        }
    }
}
