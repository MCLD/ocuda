using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IContentCategoryService
    {
        Task AddAsync(Category category);

        Task<IEnumerable<Category>> GetAllAsync();

        Task<int?> GetIdAsync(string categorySlug);

        Task<CollectionWithCount<Category>> PageAsync(BaseFilter filter);

        Task RemoveAsync(int categoryId);
    }
}
