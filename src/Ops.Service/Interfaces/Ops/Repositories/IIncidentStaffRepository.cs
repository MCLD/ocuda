using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentStaffRepository : IOpsRepository<IncidentStaff, int>
    {
        public Task<ICollection<IncidentStaff>> GetByIncidentIdAsync(int incidentId);

        public Task<IEnumerable<int>> IncidentIdsSearchAsync(IEnumerable<int> userIds);
    }
}
