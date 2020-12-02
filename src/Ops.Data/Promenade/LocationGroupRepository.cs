using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationGroupRepository
        : GenericRepository<PromenadeContext, LocationGroup>, ILocationGroupRepository
    {
        public LocationGroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationGroup> GetByIdsAsync(int groupId, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupId == groupId && _.LocationId == locationId)
                .SingleOrDefaultAsync();
        }

        public async Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == location.Id)
                .ToListAsync();
        }

        public async Task<List<LocationGroup>> GetLocationGroupsByGroupAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupId == groupId)
                .OrderBy(_ => _.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateAsync(LocationGroup locationGroup)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId != locationGroup.LocationId
                    && _.GroupId != locationGroup.GroupId)
                .AnyAsync();
        }
    }
}
