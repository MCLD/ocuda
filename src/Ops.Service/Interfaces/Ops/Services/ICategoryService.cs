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
        Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter, bool isGallery = false);
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByNameAsync(string name);
        Task<int> GetCategoryCountAsync();
        Task<Category> CreateCategoryAsync(
            int currentUserId, Category category, int[] fileTypeIds = null);
        Task<Category> EditCategoryAsync(
            int currentUserId, int id, string name, bool thumbnail = false, int[] fileTypeIds = null);
        Task DeleteCategoryAsync(int id);
        Task CreateDefaultCategories(int currentUserId, Section section);
        Task<Category> GetDefaultAsync(BlogFilter filter);
        Task<Category> GetAttachmentAsync(BlogFilter filter);
        Task<Category> GetCategoryAndFileTypesByCategoryIdAsync(int categoryId);
        Task<IEnumerable<int>> GetFileTypeIdsByCategoryIdAsync(int categoryId);
    }
}
