using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentRepository : IGenericRepository<Segment, int>
    {
        Task<List<Segment>> GetAllActiveSegments();
    }
}
