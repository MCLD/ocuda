using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IRosterService
    {
        Task<string> AddUnitMap(int unitId, int locationId);

        Task<CollectionWithCount<RosterHeader>> GetPaginatedRosterHeadersAsync(BaseFilter filter);

        Task<CollectionWithCount<UnitLocationMap>> GetUnitLocationMapsAsync(BaseFilter filter);

        Task<RosterUpdate> ImportRosterAsync(int currentUserId, string filename);

        Task<string> RemoveUnitMap(int unitId);
    }
}
