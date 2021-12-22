using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IHistoricalIncidentService
    {
        public Task<HistoricalIncident> GetAsync(int id);

        Task<DataWithCount<ICollection<HistoricalIncident>>> GetPaginatedAsync(SearchFilter filter);
    }
}
