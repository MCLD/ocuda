using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentTypeRepository : IOpsRepository<IncidentType, int>
    {
        public Task<ICollection<IncidentType>> GetActiveAsync();

        public Task<ICollection<IncidentType>> GetAllAsync();
        public Task<CollectionWithCount<IncidentType>> GetAsync(BaseFilter filter);

        public Task<IncidentType> GetAsync(string incidentTypeDescription);
    }
}
