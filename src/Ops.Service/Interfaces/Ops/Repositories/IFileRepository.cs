using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileRepository : IRepository<File, int>
    {
        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            BlogFilter filter, bool isGallery);
        Task<IEnumerable<int>> GetFileTypeIdsInUseByCategoryId(int categoryId);
        Task<IEnumerable<File>> GetByPageIdAsync(int pageId);
        Task<IEnumerable<File>> GetByPostIdAsync(int postId);
    }
}
