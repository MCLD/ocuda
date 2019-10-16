using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Ops
{
    public class LocationFeatureRepository : GenericRepository<PromenadeContext, LocationFeature, int>, ILocationFeatureRepository
    {
        public LocationFeatureRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationFeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == location.Id)
                .ToListAsync();
        }

        public async Task<List<LocationFeature>> GeAllLocationFeaturesAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateAsync(LocationFeature locationfeature)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != locationfeature.Id
                    && _.LocationId == locationfeature.LocationId
                    && _.FeatureId == locationfeature.FeatureId)
                .AnyAsync();
        }
    }
}
