using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationHoursOverrideRepository
        : GenericRepository<PromenadeContext, LocationHoursOverride>,
            ILocationHoursOverrideRepository
    {
        public LocationHoursOverrideRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursOverrideRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationHoursOverride> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<LocationHoursOverride>> GetByLocationIdAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.LocationId.HasValue || _.LocationId == locationId)
                .OrderBy(_ => _.Date)
                .ToListAsync();
        }

        public async Task<ICollection<LocationHoursOverride>> GetConflictingOverrideDatesAsync(
            LocationHoursOverride hoursOverride)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Location)
                .Where(_ => _.Id != hoursOverride.Id
                    && _.Date.Date == hoursOverride.Date
                    && (!hoursOverride.LocationId.HasValue || !_.LocationId.HasValue
                        || _.LocationId == hoursOverride.LocationId))
                .ToListAsync();
        }
    }
}
