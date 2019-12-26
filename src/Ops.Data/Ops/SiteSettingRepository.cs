using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository
        : GenericRepository<OpsContext, SiteSetting, string>, ISiteSettingRepository
    {
        public SiteSettingRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SiteSettingRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<bool> IsDuplicateKey(SiteSetting setting)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != setting.Id)
                .AnyAsync();
        }
    }
}
