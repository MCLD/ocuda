using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavigationRepository 
        : GenericRepository<PromenadeContext, Navigation>, INavigationRepository
    {
        public NavigationRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavigationRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
