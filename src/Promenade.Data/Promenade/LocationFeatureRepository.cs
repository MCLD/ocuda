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

        public async Task<LocationFeature> GetFullLocationFeatureAsync(string locationStub,
            string featureStub)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Feature)
                .Where(_ => _.Feature.Stub == featureStub
                    && _.Location.Stub == locationStub)
                .SingleOrDefaultAsync();
        }

        public async Task<List<LocationFeature>>
            GetFullLocationFeaturesAsync(string locationStub)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Feature)
                .Where(_ => _.Location.Stub == locationStub)
                .OrderByDescending(_ => _.Feature.SortOrder.HasValue)
                .ThenBy(_ => _.Feature.SortOrder)
                .ThenBy(_ => _.Feature.Name)
                .ToListAsync();
        }
    }
}
