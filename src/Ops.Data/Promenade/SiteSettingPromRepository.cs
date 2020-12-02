using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class SiteSettingPromRepository
        : GenericRepository<PromenadeContext, SiteSetting>, ISiteSettingPromRepository
    {
        public SiteSettingPromRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SiteSettingPromRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SiteSetting> FindAsync(string id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }
    }
}
