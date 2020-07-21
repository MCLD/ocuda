using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaGroupRepository : IGenericRepository<EmediaGroup>
    {
        Task<ICollection<EmediaGroup>> GetUsingSegmentAsync(int segmentId);
    }
}
