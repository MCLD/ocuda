using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRosterHeaderRepository : IOpsRepository<RosterHeader, int>
    {
        Task<int?> GetLatestIdAsync();

        Task<CollectionWithCount<RosterHeader>> GetPaginatedAsync(BaseFilter filter);
    }
}
