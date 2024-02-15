using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ICategoryService
    {
        Task<Category> CreateAsync(Category category);

        Task DeleteAsync(int id);

        Task<Category> EditAsync(Category category);

        Task<ICollection<Category>> GetAllAsync();

        Task<Category> GetByIdAsync(int id);

        Task<ICollection<string>> GetCategoryEmediasAsync(int id);

        Task<ICollection<string>> GetCategoryLanguagesAsync(int id);

        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
                                    BaseFilter filter);

        Task<CategoryText> GetTextByCategoryAndLanguageAsync(int categoryId, int languageId);

        Task SetCategoryTextAsync(CategoryText categoryText);
    }
}