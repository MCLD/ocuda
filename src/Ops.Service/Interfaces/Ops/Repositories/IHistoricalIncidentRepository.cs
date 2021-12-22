using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IHistoricalIncidentRepository : IOpsRepository<HistoricalIncident, int>
    {
        public Task<HistoricalIncident> GetHistoricalIncidentAsync(int id);

        public Task<DataWithCount<ICollection<HistoricalIncident>>>
            GetPaginatedListAsync(SearchFilter filter);
    }
}
