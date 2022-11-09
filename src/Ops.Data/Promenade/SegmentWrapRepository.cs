using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class SegmentWrapRepository
        : GenericRepository<PromenadeContext, SegmentWrap>, ISegmentWrapRepository
    {
        public SegmentWrapRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentWrapRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SegmentWrap> FindAsync(int segmentWrapId)
        {
            return await DbSet.AsNoTracking()
                .SingleOrDefaultAsync(_ => !_.IsDeleted && _.Id == segmentWrapId);
        }

        public async Task<IDictionary<string, string>> GetActiveListAsync()
        {
            return await DbSet.AsNoTracking()
                .Where(_ => !_.IsDeleted)
                .ToDictionaryAsync(k => k.Id.ToString(), v => v.Name + ' ' + v.Description);
        }

        public async Task<CollectionWithCount<SegmentWrap>> GetPaginatedAsync(BaseFilter filter)
        {
            return new CollectionWithCount<SegmentWrap>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task PermanentlyDeleteAsync(int segmentWrapId)
        {
            var segmentWrap = await DbSet.SingleOrDefaultAsync(_ => _.Id == segmentWrapId);
            if (segmentWrap == null)
            {
                throw new OcudaException("Unable to find Segment Wrap Id {segmentWrapId} to delete.");
            }
            else
            {
                DbSet.Remove(segmentWrap);
                await _context.SaveChangesAsync();
            }
        }
    }
}