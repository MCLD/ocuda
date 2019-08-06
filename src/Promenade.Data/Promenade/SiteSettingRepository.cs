using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SiteSettingRepository
        : GenericRepository<PromenadeContext, SiteSetting, int>, ISiteSettingRepository
    {
        public SiteSettingRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
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
    }
}
