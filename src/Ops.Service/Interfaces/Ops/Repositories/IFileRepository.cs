using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileRepository : IRepository<File, int>
    {
        Task<File> GetLatestByLibraryIdAsync(int id);
        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            BlogFilter filter, bool isGallery);
        Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId);
        Task<IEnumerable<File>> GetByPageIdAsync(int pageId);
        Task<IEnumerable<File>> GetByPostIdAsync(int postId);
    }
}
