using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteManagerGroupRepository
        : GenericRepository<Models.SiteManagerGroup, int>, ISiteManagerGroupRepository
    {
        public SiteManagerGroupRepository(OpsContext context,
            ILogger<SiteManagerGroupRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<bool> IsSiteManagerAsync(string group)
        {
            if(string.IsNullOrEmpty(group))
            {
                return false;
            }

            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupName == group.Trim())
                .AnyAsync();
        }
    }
}
