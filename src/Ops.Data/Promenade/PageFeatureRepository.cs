using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageFeatureRepository : GenericRepository<PromenadeContext, PageFeature>,
        IPageFeatureRepository
    {
        public PageFeatureRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
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

        public async Task<PageFeature> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageHeaderIdForPageFeatureAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == id)
                .Select(_ => _.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetPageLayoutIdForPageFeatureAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == id)
                .Select(_ => _.PageLayoutId)
                .SingleOrDefaultAsync();
        }
    }
}
