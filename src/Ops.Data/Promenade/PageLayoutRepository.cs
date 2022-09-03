using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageLayoutRepository : GenericRepository<PromenadeContext, PageLayout>,
        IPageLayoutRepository
    {
        public PageLayoutRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageLayoutRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageLayout> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<PageLayout>> GetAllForHeaderIncludingChildrenAsync(
            int headerId)
        {
            return await DbSet
                .Where(_ => _.PageHeaderId == headerId)
                .Include(_ => _.Items)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PageLayout> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<PageLayout> GetIncludingChildrenWithItemContent(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                    .ThenInclude(_ => _.BannerFeature)
                        .ThenInclude(_ => _.Items)
                .Include(_ => _.Items)
                    .ThenInclude(_ => _.Deck)
                        .ThenInclude(_ => _.Cards)
                .Include(_ => _.Items)
                    .ThenInclude(_ => _.Carousel)
                        .ThenInclude(_ => _.Items)
                .Include(_ => _.Items)
                    .ThenInclude(_ => _.PageFeature)
                        .ThenInclude(_ => _.Items)
                .Include(_ => _.Items)
                    .ThenInclude(_ => _.Segment)
                .Include(_ => _.Items)
                    .ThenInclude(_ => _.Webslide)
                        .ThenInclude(_ => _.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<PageLayout>>> GetPaginatedListForHeaderAsync(
                                    int headerId,
            BaseFilter filter)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == headerId);

            return new DataWithCount<ICollection<PageLayout>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.StartDate.HasValue)
                    .ThenByDescending(_ => _.StartDate.Value)
                    .ThenBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .Include(_ => _.Items)
                    .ToListAsync()
            };
        }
    }
}