using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileCategoryRepository : IGenericRepository<FileCategory>
    {
        Task<FileCategory> FindAsync(int fileId, int categoryId);

        Task<IEnumerable<Category>> GetCategoriesAsync(IEnumerable<int> fileIds);

        Task<IEnumerable<int>> GetCategoryIdsAsync(int fileId);
    }
}
