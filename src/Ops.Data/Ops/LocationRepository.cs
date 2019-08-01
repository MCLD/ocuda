using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Ops
{
    public class LocationRepository : GenericRepository<PromenadeContext,Location, int>, ILocationRepository
    {
        public LocationRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Location>> GeAllLocationsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Location> GetLocationByStub(string locationStub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == locationStub)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name.ToLower() == location.Name.ToLower()
                    && _.Id != location.Id )
                .AnyAsync();
        }
    }
}
