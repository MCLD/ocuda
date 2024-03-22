using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
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
