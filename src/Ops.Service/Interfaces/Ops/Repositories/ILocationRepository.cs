using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILocationRepository : IRepository<Location, int>
    {
        Task<List<Location>> GeAllLocationsAsync();
        Task<Location> GetLocationByStub(string locationStub);
        Task<bool> IsDuplicateNameAsync(Location location);
        Task<bool> IsDuplicateStubAsync(Location location);
        Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(BaseFilter filter);
    }
}
