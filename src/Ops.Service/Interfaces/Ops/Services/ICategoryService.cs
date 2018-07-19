using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICategoryService
    {
        Task<DataWithCount<ICollection<Category>>> GetPaginatedCategoryListAsync(BlogFilter filter);
        Task<ICollection<Category>> GetCategoriesAsync();
        Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter);
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> GetByNameAsync(string name);
        Task<Category> GetByNameAndSectionIdAsync(string name, int sectionId);
        Task<Category> GetAttachmentCategoryAsync(int currentUserId, int sectionId);
        Task<int> GetCategoryCountAsync();
        Task<Category> CreateCategoryAsync(int currentUserId, Category category);
        Task<Category> EditCategoryAsync(int id, string name);
        Task DeleteCategoryAsync(int id);
        Task CreateDefaultCategories(int currentUserId, int sectionId);
    }
}
