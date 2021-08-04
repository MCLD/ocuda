using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaGroupRepository : IGenericRepository<EmediaGroup>
    {
        Task<EmediaGroup> GetIncludingChildredAsync(int id);
        Task<int?> GetMaxSortOrderAsync();
        Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<EmediaGroup>> GetUsingSegmentAsync(int segmentId);
    }
}
