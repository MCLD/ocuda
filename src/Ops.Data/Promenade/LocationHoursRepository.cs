using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationHoursRepository
        : GenericRepository<PromenadeContext, LocationHours>, ILocationHoursRepository
    {
        public LocationHoursRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationHours> GetByIdsAsync(DayOfWeek dayOfWeek, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DayOfWeek == dayOfWeek && _.LocationId == locationId)
                .SingleOrDefaultAsync();
        }

        public async Task<List<LocationHours>> GetLocationHoursByLocationId(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }

        public async Task<ICollection<LocationHours>> GetWeeklyHoursAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .OrderBy(_ => _.DayOfWeek)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateDayAsync(LocationHours locationHour)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationHour.LocationId
                    && _.DayOfWeek == locationHour.DayOfWeek)
                .AnyAsync();
        }
    }
}
