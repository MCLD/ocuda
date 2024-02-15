using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavBannerRepository : GenericRepository<PromenadeContext, NavBanner>,
        INavBannerRepository
    {
        public NavBannerRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselRepository> logger) : base(repositoryFacade, logger)
        {
        }

    }
}
