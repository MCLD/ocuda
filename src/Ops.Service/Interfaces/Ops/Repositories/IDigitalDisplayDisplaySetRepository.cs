using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplayDisplaySetRepository
        : IGenericRepository<DigitalDisplayDisplaySet>
    {
        public Task<ICollection<DigitalDisplayDisplaySet>>
             GetByDisplayIdsAsync(IEnumerable<int> displayIds);

        public Task<IDictionary<int, int>> GetSetsDisplaysCountsAsync();
    }
}