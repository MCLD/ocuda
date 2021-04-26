using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class WebslideItemRepository : GenericRepository<PromenadeContext, WebslideItem>,
        IWebslideItemRepository
    {
        public WebslideItemRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<WebslideItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<WebslideItem> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<int?> GetMaxSortOrderForWebslideAsync(int webslideId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideId == webslideId)
                .MaxAsync(_ => (int?)_.Order);
        }

        public async Task<WebslideItem> GetByWebslideAndOrderAsync(int webslideId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideId == webslideId && _.Order == order)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WebslideItem>> GetWebslideSubsequentAsync(int webslideId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideId == webslideId && _.Order > order)
                .ToListAsync();
        }
    }
}
