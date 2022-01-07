using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUnitLocationMapRepository : IOpsRepository<UnitLocationMap, int>
    {
        public Task<IDictionary<int, int>> GetAllAsync();

        public Task<CollectionWithCount<UnitLocationMap>> GetPaginatedAsync(BaseFilter filter);
    }
}
