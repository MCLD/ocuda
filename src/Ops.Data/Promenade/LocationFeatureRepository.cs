﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationFeatureRepository 
        : GenericRepository<PromenadeContext, LocationFeature>, ILocationFeatureRepository
    {
        public LocationFeatureRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationFeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationFeature> GetByIdsAsync(int featureId, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FeatureId == featureId && _.LocationId == locationId)
                .SingleOrDefaultAsync();
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
                .Where(_ => _.LocationId == locationfeature.LocationId
                    && _.FeatureId == locationfeature.FeatureId)
                .AnyAsync();
        }
    }
}
