using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IProductService
    {
        Task<Product> GetBySlugAsnyc(string slug);
        Task<ProductLocationInventory> GetInventoryByProductAndLocation(int productId, int locationId);
        Task<ICollection<ProductLocationInventory>> GetLocationInventoriesForProductAsync(int productId);
        Task UpdateInventoryStatus(int productId, int locationId, ProductLocationInventory.Status status);
    }
}
