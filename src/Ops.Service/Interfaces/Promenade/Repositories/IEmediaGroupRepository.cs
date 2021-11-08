using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaGroupRepository : IGenericRepository<EmediaGroup>
    {
        Task<EmediaGroup> FindAsync(int id);
        Task<EmediaGroup> GetByOrderAsync(int order);
        Task<EmediaGroup> GetIncludingEmediaAsync(int id);
        Task<EmediaGroup> GetIncludingSegmentAsync(int id);
        Task<int?> GetMaxSortOrderAsync();
        Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedListAsync(BaseFilter filter);
        Task<List<EmediaGroup>> GetSubsequentGroupsAsync(int order);
        Task<EmediaGroup> GetUsingSegmentAsync(int segmentId);
    }
}
