using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CarouselRepository : GenericRepository<PromenadeContext, Carousel>,
        ICarouselRepository
    {
        public CarouselRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<Carousel> logger) : base(repositoryFacade, logger)
        {
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
    }
}
