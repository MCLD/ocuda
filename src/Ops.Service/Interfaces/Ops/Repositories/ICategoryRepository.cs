using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICategoryRepository : IOpsRepository<Category, int>
    {
        Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId);
        Task<Category> GetCategoryBySlugAsync(string stub);
        Task<bool> SectionHasCategoryAsync(int categoryId, int sectionId);
    }
}
