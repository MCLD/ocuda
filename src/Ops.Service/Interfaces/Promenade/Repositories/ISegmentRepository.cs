using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISegmentRepository : IGenericRepository<Segment>
    {
        Task<Segment> FindAsync(int id);

        Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter);

        Task<ICollection<Segment>> GetAllActiveSegmentsAsync();
        Task<int?> GetPageHeaderIdForSegmentAsync(int id);
        Task<int?> GetPageLayoutIdForSegmentAsync(int id);
    }
}
