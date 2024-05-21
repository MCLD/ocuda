using System;
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
    public class FeatureRepository
        : GenericRepository<PromenadeContext, Feature>, IFeatureRepository
    {
        public FeatureRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<FeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int> CountAsync(FeatureFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            return await ApplyFilters(filter)
                .CountAsync();
        }

        public async Task<Feature> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<List<Feature>> GetAllFeaturesAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<ICollection<Feature>> GetByIdsAsync(IEnumerable<int> featureIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => featureIds.Contains(_.Id))
                .ToListAsync();
        }

        public async Task<Feature> GetBySegmentIdAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.NameSegmentId == segmentId
                    || _.TextSegmentId == segmentId);
        }

        public async Task<Feature> GetBySlugAsync(string slug)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Stub == slug);
        }

        public async Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Feature>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderByDescending(_ => _.IsAtThisLocation)
                    .ThenByDescending(_ => _.SortOrder.HasValue)
                    .ThenBy(_ => _.SortOrder)
                    .ThenBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> IsDuplicateNameAsync(Feature feature)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name == feature.Name && _.Id != feature.Id)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicateStubAsync(Feature feature)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != feature.Id && _.Stub == feature.Stub)
                .AnyAsync();
        }

        public async Task<ICollection<Feature>> PageAsync(FeatureFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            return await ApplyFilters(filter)
                .OrderBy(_ => _.Name)
                .ApplyPagination(filter)
                .ToListAsync();
        }

        public async Task UpdateName(int featureId, string newName)
        {
            var feature = await DbSet.SingleOrDefaultAsync(_ => _.Id == featureId);
            feature.Name = newName;
            Update(feature);
            await SaveAsync();
        }

        private IQueryable<Feature> ApplyFilters(FeatureFilter filter)
        {
            var items = DbSet.AsNoTracking();

            if (filter.FeatureIds?.Count > 0)
            {
                items = items.Where(_ => !filter.FeatureIds.Contains(_.Id));
            }

            return items;
        }
    }
}