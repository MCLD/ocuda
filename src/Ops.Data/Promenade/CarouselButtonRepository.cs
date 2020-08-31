using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CarouselButtonRepository : GenericRepository<PromenadeContext, CarouselButton>,
        ICarouselButtonRepository
    {
        public CarouselButtonRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselButtonRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselButton> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<int?> GetMaxSortOrderForItemAsync(int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItemId == itemId)
                .MaxAsync(_ => (int?)_.Order);
        }

        public async Task<CarouselButton> GetByItemAndOrderAsync(int itemId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItemId == itemId && _.Order == order)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CarouselButton>> GetCarouselSubsequentAsync(int itemId, int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItemId == itemId && _.Order > order)
                .ToListAsync();
        }
    }
}
