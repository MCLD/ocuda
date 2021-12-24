using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product> GetActiveBySlugAsync(string slug);

        Task<Product> GetByIdAsync(int productId);

        Task<ICollection<Product>> GetBySegmentIdAsync(int segmentId);

        Task<ICollectionWithCount<Product>> GetPaginatedListAsync(BaseFilter filter);
    }
}
