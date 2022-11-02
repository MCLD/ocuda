using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentRepository : IOpsRepository<Incident, int>
    {
        public Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter);

        public Task<Incident> GetRelatedAsync(int incidentId);

        public Task SetVisibilityAsync(int incidentId, int userId, bool isVisible);
    }
}