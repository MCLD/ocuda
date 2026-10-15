using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ISectionCategoryRepository : IGenericRepository<SectionCategory>
    {
        Task<IEnumerable<Category>> GetCategoriesAsync(int sectionId);

        Task<IEnumerable<Section>> GetSectionsAsync(int categoryId);

        Task RemoveAsync(int sectionId, int categoryId);
    }
}
