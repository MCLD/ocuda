using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IProductService
    {
        Task<ICollection<string>> BulkInventoryStatusUpdateAsync(int productId,
            bool addValues,
            IDictionary<int, int> adjustments);

        Task<ICollection<Product>> GetBySegmentIdAsync(int segmentId);

        Task<Product> GetBySlugAsync(string slug);

        Task<Product> GetBySlugAsync(string slug, bool ignoreActiveFlag);

        Task<ProductLocationInventory> GetInventoryByProductAndLocationAsync(int productId, int locationId);

        Task<ICollection<ProductLocationInventory>> GetLocationInventoriesForProductAsync(int productId);

        Task<ICollectionWithCount<Product>> GetPaginatedListAsync(BaseFilter filter);

        Task LinkSegment(int productId, int segmentId);

        Task<IDictionary<int, int>> ParseInventoryAsync(int productId, string filename);

        Task SetActiveLocation(string productSlug, int locationId, bool isActive);

        Task UnlinkSegment(int productId);

        Task UpdateInventoryStatusAsync(int productId, int locationId, int itemCount);

        Task<Product> UpdateProductAsync(Product product);

        Task UpdateThreshholdAsync(int productId, int locationId, int threshholdValue);
    }
}
