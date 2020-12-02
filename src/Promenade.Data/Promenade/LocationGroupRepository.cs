using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationGroupRepository
        : GenericRepository<PromenadeContext, LocationGroup>, ILocationGroupRepository
    {
        public LocationGroupRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationGroup> GetByIdsAsync(int groupId, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupId == groupId && _.LocationId == locationId)
                .SingleOrDefaultAsync();
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
                .OrderBy(_ => _.DisplayOrder)
                .ToListAsync();
        }
    }
}
