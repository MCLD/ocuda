using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class ClaimGroupRepository
        : GenericRepository<Models.ClaimGroup, int>, IClaimGroupRepository
    {
        public ClaimGroupRepository(OpsContext context,
            ILogger<ClaimGroupRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<bool> IsClaimGroup(string claim, string group)
        {
            if(string.IsNullOrEmpty(group) || string.IsNullOrEmpty(claim))
            {
                return false;
            }

            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupName == group.Trim() && _.ClaimType == claim)
                .AnyAsync();
        }

        public async Task<ICollection<string>> GroupsForClaim(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return null;
            }

            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupName == group.Trim())
                .Select(_ => _.ClaimType)
                .ToListAsync();
        }
    }
}
