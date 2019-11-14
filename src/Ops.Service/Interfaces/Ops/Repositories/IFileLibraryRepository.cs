using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileLibraryRepository : IRepository<FileLibrary, int>
    {
        Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedListAsync(BlogFilter filter);
        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);
        List<FileLibrary> GetFileLibrariesBySectionId(int sectionId);
        Task RemoveLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId);
        Task AddLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId);
    }
}
