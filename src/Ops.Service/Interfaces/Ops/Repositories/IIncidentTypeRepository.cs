using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIncidentTypeRepository : IOpsRepository<IncidentType, int>
    {
        public Task<ICollection<IncidentType>> GetAllAsync();
    }
}
