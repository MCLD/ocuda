using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ICategoryService
    {
        Task<ICollection<Category>> GetAllCategories();
        Category GetByClass(string categoryClass);
        Task AddCategory(Category category);
        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter);
        Task UpdateCategory(Category category);
        Task DeleteAsync(int id);
    }
}
