using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRosterDivisionRepository : IOpsRepository<RosterDivision, int>
    {
        public Task<ICollection<RosterDivision>> GetAllAsync();
    }
}