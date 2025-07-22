using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISegmentRepository : IGenericRepository<Segment>
    {
        Task<int> CountByWrapAsync(int segmentWrapId);

        Task<Segment> FindAsync(int id);

        Task<ICollection<Segment>> GetAllActiveSegmentsAsync();

        Task<IDictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids);

        Task<int?> GetPageHeaderIdForSegmentAsync(int id);

        Task<int?> GetPageLayoutIdForSegmentAsync(int id);

        Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(BaseFilter filter);
    }
}