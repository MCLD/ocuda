using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRosterLocationRepository : IOpsRepository<RosterLocation, int>
    {
        public Task<ICollection<RosterLocation>> GetAllAsync();
    }
}