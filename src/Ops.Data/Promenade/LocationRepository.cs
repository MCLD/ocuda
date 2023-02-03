using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

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
            if (entity == null)
            {
                return null;
            }
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<Dictionary<int, string>> GetAllLocationsIdNameAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted)
                .OrderBy(_ => _.Name)
                .ToDictionaryAsync(k => k.Id, v => v.Name);
        }

        public async Task<Location> GetLocationByStub(string locationStub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == locationStub && !_.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(
            LocationFilter filter)
        {
            var query = ApplyFilters(filter);

            return new DataWithCount<ICollection<Location>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<ICollection<Location>> GetUsingSegmentAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted
                    && (_.DescriptionSegmentId == segmentId
                    || _.PostFeatureSegmentId == segmentId
                    || _.PreFeatureSegmentId == segmentId
                    || _.HoursSegmentId == segmentId))
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateNameAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != location.Id && _.Name == location.Name && !_.IsDeleted)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicateStubAsync(Location location)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != location.Id && _.Stub == location.Stub && !_.IsDeleted)
                .AnyAsync();
        }

        public async Task<IEnumerable<int>> SearchIdsByNameAsync(string searchText)
        {
            return await DbSet
                 .AsNoTracking()
                 .Where(_ => _.Name.Contains(searchText) && !_.IsDeleted)
                 .Select(_ => _.Id)
                 .ToListAsync();
        }

        private IQueryable<Location> ApplyFilters(LocationFilter filter)
        {
            var items = DbSet.AsNoTracking();

            return filter.IsDeleted
                ? items.Where(_ => _.IsDeleted)
                : items.Where(_ => !_.IsDeleted);
        }
    }
}