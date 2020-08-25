using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILinkService
    {
        Task<int> GetLinkCountAsync();
        Task<ICollection<Link>> GetLinksAsync();
        Task<Link> GetByIdAsync(int id);
        Task<Link> GetLatestByLibraryIdAsync(int id);
        Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter);
        Task<Link> CreateAsync(Link link);
        Task<Link> EditAsync(Link link);
        Task DeleteAsync(int id);
        Task<List<Link>> GetLinkLibraryLinksAsync(int id);
        Task<LinkLibrary> GetLibraryByIdAsync(int id);

        Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter);

        Task<LinkLibrary> CreateLibraryAsync(LinkLibrary library, int sectionId);
        Task<LinkLibrary> UpdateLibraryAsync(LinkLibrary library);
        Task DeleteLibraryAsync(int id);
        Task<List<LinkLibrary>> GetLinkLibrariesBySectionAsync(int sectionId);
    }
}
