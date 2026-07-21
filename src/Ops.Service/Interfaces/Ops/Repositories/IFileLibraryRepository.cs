using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileLibraryRepository : IOpsRepository<FileLibrary, int>
    {
        Task AddLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId);

        Task<ICollection<FileLibrary>> GetBySectionAsync(int sectionId, bool? isFeatured);

        Task<FileLibrary> GetBySectionIdSlugAsync(int sectionId, string slug);

        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);

        Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedListAsync(BlogFilter filter);

        Task RemoveLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId);
    }
}
