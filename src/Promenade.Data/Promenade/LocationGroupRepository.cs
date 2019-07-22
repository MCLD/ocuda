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
    public class LocationGroupRepository : GenericRepository<LocationGroup, int>, ILocationGroupRepository
    {
        public LocationGroupRepository(PromenadeContext context, 
            ILogger<LocationHoursRepository> logger) : base(context, logger)
        {
        }

        public async Task<List<LocationGroup>> GetGroupByLocationIdAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }

        public async Task<List<LocationGroup>> GetLocationsByGroupIdAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupId == groupId)
                .ToListAsync();
        }
    }
}
