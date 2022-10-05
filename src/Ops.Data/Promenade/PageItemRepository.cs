using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageItemRepository : GenericRepository<PromenadeContext, PageItem>,
        IPageItemRepository
    {
        public PageItemRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageItem> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<PageItem> GetByLayoutAndOrderAsync(int layoutId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayoutId == layoutId && _.Order == order)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetImageFeatureUseCountAsync(int imageFeatureId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.BannerFeatureId == imageFeatureId
                    || _.PageFeatureId == imageFeatureId
                    || _.WebslideId == imageFeatureId)
                .CountAsync();
        }

        public async Task<PageLayout> GetLayoutForItemAsync(int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == itemId)
                .Select(_ => _.PageLayout)
                .SingleOrDefaultAsync();
        }

        public async Task<List<PageItem>> GetLayoutSubsequentAsync(int layoutId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayoutId == layoutId && _.Order > order)
                .ToListAsync();
        }

        public async Task<int?> GetMaxSortOrderForLayoutAsync(int layoutId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayoutId == layoutId)
                .MaxAsync(_ => (int?)_.Order);
        }

        public async Task<int> RemoveByDeckIdAsync(int deckId)
        {
            var pageItems = DbSet.Where(_ => _.DeckId == deckId);
            var pageLayoutId = pageItems.FirstOrDefault()?.PageLayoutId ?? 0;
            DbSet.RemoveRange(pageItems);
            await _context.SaveChangesAsync();
            return pageLayoutId;
        }
    }
}