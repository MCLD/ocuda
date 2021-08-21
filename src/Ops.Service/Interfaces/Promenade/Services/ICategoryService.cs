using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ICategoryService
    {
        Task<ICollection<Category>> GetAllAsync();
        Category GetByClass(string categoryClass);
        Task AddCategory(Category category);
        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter);
        Task UpdateCategory(Category category);
        Task DeleteAsync(int id);
    }
}
