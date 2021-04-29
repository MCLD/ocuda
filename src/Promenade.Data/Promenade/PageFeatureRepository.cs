using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PageFeatureRepository : GenericRepository<PromenadeContext, PageFeature>,
        IPageFeatureRepository
    {
        public PageFeatureRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PageFeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageFeature> FindAsync(int id)
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
