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

        public async Task<LocationFeature> GetFullLocationFeatureAsync(int locationId,
            string featureStub)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Feature)
                .Where(_ => !_.Location.IsDeleted
                    && _.Feature.Stub == featureStub
                    && _.Location.Id == locationId)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<LocationFeature>> GetFullLocationFeaturesAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Feature)
                .Where(_ => !_.Location.IsDeleted && _.Location.Id == locationId)
                .OrderByDescending(_ => _.Feature.SortOrder.HasValue)
                .ThenBy(_ => _.Feature.SortOrder)
                .ThenBy(_ => _.Feature.Name)
                .ToListAsync();
        }
    }
}