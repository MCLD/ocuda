using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IRosterDetailRepository : IRepository<RosterDetail, int>
    {
        Task AddRangeAsync(IEnumerable<RosterDetail> rosterDetails);
    }
}
