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
    public class SegmentRepository : GenericRepository<PromenadeContext, Segment>, ISegmentRepository
    {
        public SegmentRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int> CountByWrapAsync(int segmentWrapId)
        {
            return await DbSet
                .AsNoTracking()
                .CountAsync(_ => _.SegmentWrapId == segmentWrapId);
        }

        public async Task<Segment> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<Segment>> GetAllActiveSegmentsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .ToListAsync();
        }

        public async Task<Segment> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.SegmentText)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<IDictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => ids.Contains(_.Id))
                .ToDictionaryAsync(k => k.Id, v => v.Name);
        }

        public async Task<int?> GetPageHeaderIdForSegmentAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.SegmentId == id)
                .Select(_ => (int?)_.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageLayoutIdForSegmentAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.SegmentId == id)
                .Select(_ => (int?)_.PageLayoutId)
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
                                    BaseFilter filter)
        {
            return new DataWithCount<ICollection<Segment>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task UpdateWrapAsync(int segmentId, int? segmentWrapId)
        {
            var segment = await DbSet.FindAsync(segmentId);
            segment.SegmentWrapId = segmentWrapId;
            DbSet.Update(segment);
            await _context.SaveChangesAsync();
        }
    }
}