using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISegmentRepository : IGenericRepository<Segment>
    {
        Task<Segment> FindAsync(int id);

        Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter);

        Task<ICollection<Segment>> GetAllActiveSegmentsAsync();
    }
}
