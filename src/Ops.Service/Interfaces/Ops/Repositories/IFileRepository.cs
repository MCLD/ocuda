using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileRepository : IOpsRepository<File, int>
    {
        Task<IEnumerable<int>> GetFileLibraryFileIdsAsync(int fileLibraryId);

        Task<ICollection<File>> GetFileLibraryFilesAsync(int fileLibraryId);

        Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int fileLibraryId);

        Task<File> GetLatestByLibraryIdAsync(int fileLibraryId);

        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(FilesFilter filter);
    }
}
