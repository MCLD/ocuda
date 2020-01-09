using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationFeatureRepository
        : GenericRepository<PromenadeContext, LocationFeature>, ILocationFeatureRepository
    {
        public LocationFeatureRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationFeature> GetByIdsAsync(int featureId, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FeatureId == featureId && _.LocationId == locationId)
                .SingleOrDefaultAsync();
        }

        public async Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }
    }
}
