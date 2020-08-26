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
    }
}
