using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplaySetRepository : IOpsRepository<DigitalDisplaySet, int>
    {
        public Task<ICollection<DigitalDisplaySet>> GetAllAsync();

        public Task<DigitalDisplaySet> GetByNameAsync(string setName);

        public Task<IDictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> setIds);
    }
}
