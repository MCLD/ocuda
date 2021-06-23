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
    public class CarouselRepository : GenericRepository<PromenadeContext, Carousel>,
        ICarouselRepository
    {
        public CarouselRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Carousel> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<Carousel>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Carousel> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .ThenInclude(_ => _.Buttons)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<Carousel> GetIncludingChildrenWithLabelsAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .ThenInclude(_ => _.Buttons)
                .ThenInclude(_ => _.Label)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageHeaderIdForCarouselAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.CarouselId == id)
                .Select(_ => _.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageLayoutIdForCarouselAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.CarouselId == id)
                .Select(_ => (int?)_.PageLayoutId)
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(
                                            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<Carousel>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Id)
                    .ApplyPagination(filter)
                    .Include(_ => _.Items)
                    .ToListAsync()
            };
        }
    }
}