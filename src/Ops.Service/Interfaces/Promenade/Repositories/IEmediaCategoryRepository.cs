using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaCategoryRepository : IGenericRepository<EmediaCategory>
    {
        Task<ICollection<EmediaCategory>> GetByCategoryIdAsync(int categoryId);
        Task<ICollection<EmediaCategory>> GetAllForGroupAsync(int groupId);
        Task<ICollection<int>> GetCategoryIdsForEmediaAsync(int emediaId);
        void RemoveByEmediaAndCategories(int emediaId, ICollection<int> categoryIds);
        Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId);
        Task<ICollection<EmediaCategory>> GetAllForEmediaAsync(int emediaId);
        Task<ICollection<string>> GetEmediasForCategoryAsync(int categoryId);
    }
}
