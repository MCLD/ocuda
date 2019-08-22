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
    public class LocationHoursRepository : GenericRepository<PromenadeContext, LocationHours, int>, ILocationHoursRepository
    {
        public LocationHoursRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LinkRepository> logger) : base(repositoryFacade, logger)
        {
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
                .Where(_ => _.Id != locationHour.Id && _.LocationId == locationHour.LocationId && _.DayOfWeek == locationHour.DayOfWeek)
                .AnyAsync();
        }
    }
}
