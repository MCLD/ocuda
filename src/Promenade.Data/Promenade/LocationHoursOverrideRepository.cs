using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationHoursOverrideRepository 
        : GenericRepository<PromenadeContext, LocationHoursOverride, int>, ILocationHoursOverrideRepository
    {
        public LocationHoursOverrideRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursOverrideRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationHoursOverride> GetByDateAsync(int locationId, DateTime date)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Date.Date == date.Date
                    && (_.LocationId == locationId || !_.LocationId.HasValue))
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<LocationHoursOverride>> GetBetweenDatesAsync(int locationId,
            DateTime startDate,
            DateTime endDate)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Date.Date >= startDate.Date && _.Date.Date <= endDate.Date
                    && (_.LocationId == locationId || !_.LocationId.HasValue))
                .ToListAsync();
        }
    }
}
