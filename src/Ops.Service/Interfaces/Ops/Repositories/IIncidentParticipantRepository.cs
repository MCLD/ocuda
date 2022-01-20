using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentParticipantRepository : IOpsRepository<IncidentParticipant, int>
    {
        public Task<ICollection<IncidentParticipant>> GetByIncidentIdAsync(int incidentId);

        public Task<IEnumerable<int>> IncidentIdsSearchAsync(string searchText);
    }
}
