using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IProductService
    {
        Task<ICollection<string>> BulkInventoryStatusUpdateAsync(int productId,
            bool addValues,
            IDictionary<int, int> adjustments);

        Task<Product> GetBySlugAsync(string slug);

        Task<ProductLocationInventory> GetInventoryByProductAndLocationAsync(int productId, int locationId);

        Task<ICollection<ProductLocationInventory>> GetLocationInventoriesForProductAsync(int productId);

        Task<IDictionary<int, int>> ParseInventory(int productId, string filename);

        Task UpdateInventoryStatusAsync(int productId, int locationId, int itemCount);

        Task UpdateThreshholdAsync(int productId, int locationId, int threshholdValue);
    }
}