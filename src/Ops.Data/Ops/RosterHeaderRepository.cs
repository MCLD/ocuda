using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterHeaderRepository
        : GenericRepository<RosterHeader, int>, IRosterHeaderRepository
    {
        public RosterHeaderRepository(OpsContext context, ILogger<RosterHeaderRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<int?> GetLatestIdAsync()
        {
            var latest = await DbSet
                .AsNoTracking()
                .OrderByDescending(_ => _.CreatedAt)
                .FirstOrDefaultAsync();

            if(latest != null)
            {
                return latest.Id;
            }
            else
            {
                return null;
            }
        }
    }
}