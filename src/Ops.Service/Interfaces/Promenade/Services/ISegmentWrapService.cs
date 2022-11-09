using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISegmentWrapService
    {
        public Task AddAsync(SegmentWrap segmentWrap);

        public Task<bool> DisableAsync(int segmentWrapId);

        public Task<SegmentWrap> FindAsync(int segmentWrapId);

        public Task<IDictionary<string, string>> GetActiveListAsync();

        public Task<CollectionWithCount<SegmentWrap>> GetPaginatedAsync(BaseFilter filter);

        public Task UpdateAsync(SegmentWrap segmentWrap);
    }
}