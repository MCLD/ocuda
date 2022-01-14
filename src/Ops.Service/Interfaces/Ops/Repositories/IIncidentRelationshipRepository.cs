using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentRelationshipRepository : IOpsRepository<IncidentRelationship, int>
    {
        public Task<ICollection<IncidentRelationship>> GetByIncidentIdAsync(int incidentId);
    }
}
