using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SiteSettingRepository
        : GenericRepository<PromenadeContext, SiteSetting>, ISiteSettingRepository
    {
        public SiteSettingRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SiteSettingRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SiteSetting> FindAsync(string key)
        {
            var entity = await DbSet.FindAsync(key);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }
    }
}
