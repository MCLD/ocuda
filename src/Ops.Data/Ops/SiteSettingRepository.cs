using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository
        : GenericRepository<SiteSetting, int>, ISiteSettingRepository
    {
        public SiteSettingRepository(OpsContext context, ILogger<SiteSettingRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<SiteSetting> FindByKeyAsync(string key)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Key == key)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateKey(SiteSetting siteSetting)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Key == siteSetting.Key
                         && _.Id != siteSetting.Id)
                .AnyAsync();
        }
    }
}
