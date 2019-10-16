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
    public class LocationGroupRepository : GenericRepository<PromenadeContext,LocationGroup, int>, ILocationGroupRepository
    {
        public LocationGroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == location.Id)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateAsync(LocationGroup locationGroup)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != locationGroup.Id
                    && _.LocationId != locationGroup.LocationId
                    && _.GroupId != locationGroup.GroupId)
                .AnyAsync();
        }
    }
}
