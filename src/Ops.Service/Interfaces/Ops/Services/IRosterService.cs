using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Interfaces;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IRosterService
    {
        Task<RosterComparison> CompareAsync(int rosterHeaderId, bool applyChanges);

        Task DisableHeaderAsync(int rosterHeaderId);

        Task<IEnumerable<IRosterLocationMapping>> GetDivisionsAsync();

        Task<IEnumerable<IRosterLocationMapping>> GetLocationsAsync();

        Task<CollectionWithCount<RosterHeader>> GetPaginatedRosterHeadersAsync(BaseFilter filter);

        Task<RosterUpdate> ImportRosterAsync(int currentUserId, string filename);

        Task UpdateDivisionMappingAsync(int divisionId, int? mappingId);

        Task UpdateLocationMappingAsync(int locationId, int? mappingId);
    }
}