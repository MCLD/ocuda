using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageFeatureItemRepository : GenericRepository<PromenadeContext, PageFeatureItem>,
        IPageFeatureItemRepository
    {
        public PageFeatureItemRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageFeatureItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageFeatureItem> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<PageFeatureItem> GetByFeatureAndOrderAsync(int featureId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == featureId && _.Order == order)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<PageFeatureItem>> GetByPageFeatureAsync(int featureId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == featureId)
                .ToListAsync();
        }

        public async Task<int?> GetMaxSortOrderForPageFeatureAsync(int featureId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == featureId)
                .MaxAsync(_ => (int?)_.Order);
        }

        public async Task<List<PageFeatureItem>> GetPageFeatureSubsequentAsync(int pageFeatureId,
            int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == pageFeatureId && _.Order > order)
                .ToListAsync();
        }
    }
}