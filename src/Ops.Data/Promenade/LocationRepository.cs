using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationRepository
        : GenericRepository<PromenadeContext, Location>, ILocationRepository
    {
        public LocationRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Location> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<List<Location>> GeAllLocationsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Location>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }


        public async Task<Location> GetLocationByStub(string locationStub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == locationStub)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> IsDuplicateStubAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != location.Id && _.Stub == location.Stub)
                .AnyAsync();
        }
        public async Task<bool> IsDuplicateNameAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != location.Id && _.Name == location.Name)
                .AnyAsync();
        }
    }
}
