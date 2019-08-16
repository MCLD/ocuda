using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository
        : GenericRepository<OpsContext, SiteSetting, int>, ISiteSettingRepository
    {
        public SiteSettingRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SiteSettingRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SiteSetting> FindByKeyAsync(string key)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Key == key)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateKey(SiteSetting setting)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Key == setting.Key
                         && _.Id != setting.Id)
                .AnyAsync();
        }
    }
}
