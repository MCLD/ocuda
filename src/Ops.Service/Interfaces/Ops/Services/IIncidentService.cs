using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IIncidentService
    {
        public Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter);
        public Task<IDictionary<int, string>> GetIncidentTypesAsync();
    }
}
