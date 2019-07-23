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
    public class LocationHoursRepository 
        : GenericRepository<PromenadeContext, LocationHours, int>, ILocationHoursRepository
    {
        public LocationHoursRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationHours> GetByDayOfWeek(int locationId, DateTime date)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DayOfWeek == date.DayOfWeek && _.LocationId == locationId)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<LocationHours>> GetWeeklyHoursAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .OrderBy(_ => _.DayOfWeek)
                .ToListAsync();
        }
    }
}
