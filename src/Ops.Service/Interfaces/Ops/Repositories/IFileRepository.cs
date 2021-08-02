using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileRepository : IOpsRepository<File, int>
    {
        Task<ICollection<File>> GetFileLibraryFilesAsync(int id);

        Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId);

        Task<File> GetLatestByLibraryIdAsync(int id);

        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter);
    }
}