using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationProductMapRepository
        : GenericRepository<PromenadeContext, LocationProductMap>, ILocationProductMapRepository
    {
        public LocationProductMapRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationProductMap> FindAsync(int locationProductMapId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == locationProductMapId);
        }

        public async Task<IEnumerable<LocationProductMap>> GetByProductAsync(int productId)
        {
            return await DbSet
             .AsNoTracking()
             .OrderBy(_ => _.ImportLocation)
             .ToListAsync();
        }
    }
}