using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplaySetRepository : IOpsRepository<DigitalDisplaySet, int>
    {
        public Task<ICollection<DigitalDisplaySet>> GetAllAsync();

        public Task<DigitalDisplaySet> GetByName(string setName);
    }
}