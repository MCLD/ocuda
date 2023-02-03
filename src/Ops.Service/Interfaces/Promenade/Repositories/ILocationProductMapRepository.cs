using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationProductMapRepository : IGenericRepository<LocationProductMap>
    {
        Task<LocationProductMap> FindAsync(int locationProductMapId);

        Task<IEnumerable<LocationProductMap>> GetByProductAsync(int productId);

        void RemoveForLocation(int locationId);
    }
}