using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}
