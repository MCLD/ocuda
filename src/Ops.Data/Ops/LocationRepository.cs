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
    public class LocationRepository : GenericPromenadeRepository<Location, int>, ILocationRepository
    {
        public LocationRepository(PromenadeContext context, ILogger<LocationRepository> logger)
    : base(context, logger)
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

    }
}
