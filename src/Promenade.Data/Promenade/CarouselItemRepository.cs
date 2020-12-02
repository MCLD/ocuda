using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CarouselItemRepository : GenericRepository<PromenadeContext, CarouselItem>,
        ICarouselItemRepository
    {
        public CarouselItemRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselItem> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselItem> GetForLayoutIncludingChildrenByIdAsync(int itemId,
            int layoutId)
        {
            return await _context.PageLayouts
                .Where(_ => _.Id == layoutId)
                .SelectMany(_ => _.Items)
                .Where(_ => _.CarouselId.HasValue)
                .SelectMany(_ => _.Carousel.Items)
                .Where(_ => _.Id == itemId)
                .Include(_ => _.Buttons)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
    }
}
