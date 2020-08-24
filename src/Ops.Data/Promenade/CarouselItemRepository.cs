using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CarouselItemRepository : GenericRepository<PromenadeContext, CarouselItem>,
        ICarouselItemRepository
    {
        public CarouselItemRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselItem> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<int?> GetMaxSortOrderForCarouselAsync(int carouselId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselId == carouselId)
                .MaxAsync(_ => (int?)_.Order);
        }
    }
}
