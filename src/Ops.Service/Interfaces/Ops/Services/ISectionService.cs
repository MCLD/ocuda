using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISectionService
    {
        Task AddCategoryAsync(int sectionId, int categoryId);

        Task CreateSectionAsync(Section section);

        Task DeleteSectionAsync(string sectionSlug);

        Task<ICollection<Section>> GetAllAsync();

        Task<IEnumerable<Section>> GetByCategoryId(int categoryId);

        Task<Section> GetByIdAsync(int id);

        Task<ICollection<Section>> GetByNamesAsync(ICollection<string> names);

        Task<Section> GetBySlugAsync(string slug);

        Task<IEnumerable<Category>> GetCategoriesAsync(int sectionId);

        Task<int> GetHomeSectionIdAsync();

        Task<ICollection<Section>> GetManagedByCurrentUserAsync();

        Task RemoveCategoryAsync(int sectionId, int categoryId);
    }
}
