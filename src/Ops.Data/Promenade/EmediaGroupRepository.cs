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
    public class EmediaGroupRepository
        : GenericRepository<PromenadeContext, EmediaGroup>, IEmediaGroupRepository
    {
        public EmediaGroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<EmediaGroup> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<EmediaGroup> GetIncludingChildredAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Emedias)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<EmediaGroup> GetByOrderAsync(int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SortOrder == order)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<EmediaGroup>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet
                    .OrderBy(_ => _.SortOrder)
                    .ApplyPagination(filter)
                    .Include(_ => _.Emedias)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task<int?> GetMaxSortOrderAsync()
        {
            return await DbSet
                .AsNoTracking()
                .MaxAsync(_ => (int?)_.SortOrder);
        }

        public async Task<List<EmediaGroup>> GetSubsequentGroupsAsync(int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SortOrder > order)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaGroup>> GetUsingSegmentAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId)
                .ToListAsync();
        }
    }
}
