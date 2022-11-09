using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISegmentWrapRepository : IGenericRepository<SegmentWrap>
    {
        public Task<SegmentWrap> FindAsync(int segmentWrapId);

        public Task<IDictionary<string, string>> GetActiveListAsync();

        public Task<CollectionWithCount<SegmentWrap>> GetPaginatedAsync(BaseFilter filter);

        public Task PermanentlyDeleteAsync(int segmentWrapId);
    }
}