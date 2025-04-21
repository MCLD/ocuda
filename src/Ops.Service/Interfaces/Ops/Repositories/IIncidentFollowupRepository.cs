using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentFollowupRepository : IOpsRepository<IncidentFollowup, int>
    {
        public Task<ICollection<IncidentFollowup>> GetByIncidentIdAsync(int incidentId);

        public Task<IEnumerable<int>> IncidentIdsSearchAsync(string searchText);
    }
}
