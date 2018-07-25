using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository 
        : GenericRepository<Models.SiteSetting, int>, ISiteSettingRepository
    {
        public SiteSettingRepository(OpsContext context, ILogger<SiteSettingRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<SiteSetting> FindByKeyAsync(string key)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => string.Equals(_.Key, key, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateKey(string key)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => string.Equals(_.Key, key, StringComparison.OrdinalIgnoreCase))
                .AnyAsync();
        }
    }
}
