using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileLibraryRepository : IRepository<FileLibrary, int>
    {
        void RemoveLibraryFileTypes(IEnumerable<FileLibraryFileType> libraryFileTypes);
        Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedListAsync(BlogFilter filter);
        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);
        Task RemoveSectionFileLibraryAsync(SectionFileLibrary sectionFilelibrary);
        Task<SectionFileLibrary> AddSectionFileLibraryAsync(SectionFileLibrary sectionFileLibrary);
        List<FileLibrary> GetFileLibrariesBySectionId(int sectionId);
        SectionFileLibrary GetSectionFileLibraryByLibraryId(int libId);
    }
}
