using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICategoryRepository : IOpsRepository<Category, int>
    {
        Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId);

        Task<Category> GetCategoryBySlugAsync(string slug);

        Task<CollectionWithCount<Category>> PageAsync(BaseFilter filter);

        Task<bool> SectionHasCategoryAsync(int categoryId, int sectionId);
    }
}
