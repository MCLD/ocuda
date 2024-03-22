using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationInteriorImageRepository : GenericRepository<PromenadeContext, LocationInteriorImage>, 
        ILocationInteriorImageRepository
    {
        public LocationInteriorImageRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationInteriorImageRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
