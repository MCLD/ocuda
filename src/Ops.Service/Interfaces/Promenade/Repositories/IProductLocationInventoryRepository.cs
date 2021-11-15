using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IProductLocationInventoryRepository
        : IGenericRepository<ProductLocationInventory>
    {
        Task<ProductLocationInventory> GetByProductAndLocationAsync(int productId, int locationId);
        Task<ICollection<ProductLocationInventory>> GetForProductAsync(int productId);
    }
}