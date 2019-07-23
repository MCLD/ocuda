using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationFeatureRepository : GenericRepository<LocationFeature, int>, ILocationFeatureRepository
    {
        public LocationFeatureRepository(PromenadeContext context,
            ILogger<LocationHoursRepository> logger) : base(context, logger)
        {
        }

        public async Task<List<LocationFeature>> GetLocationFeaturesById(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }
    }
}
