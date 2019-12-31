using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationRepository
        : GenericRepository<PromenadeContext, Location, int>, ILocationRepository
    {
        public LocationRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Location>> GetAllLocations()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<Location> GetLocationByStub(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub)
                .SingleOrDefaultAsync();
        }
    }
}
