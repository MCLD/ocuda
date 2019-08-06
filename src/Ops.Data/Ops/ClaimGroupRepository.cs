using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class ClaimGroupRepository
        : GenericRepository<ClaimGroup, int>, IClaimGroupRepository
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
    }
}
