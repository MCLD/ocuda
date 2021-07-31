using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILinkService
    {
        Task<Link> CreateAsync(Link link);

        Task<LinkLibrary> CreateLibraryAsync(LinkLibrary library, int sectionId);

        Task DeleteAsync(int id);

        Task DeleteLibraryAsync(int id);

        Task<Link> EditAsync(Link link);

        Task<Link> GetByIdAsync(int id);

        Task<List<LinkLibrary>> GetBySectionIdAsync(int sectionId);

        Task<Link> GetLatestByLibraryIdAsync(int id);

        Task<LinkLibrary> GetLibraryByIdAsync(int id);

        Task<int> GetLinkCountAsync();

        Task<List<Link>> GetLinkLibraryLinksAsync(int id);

        Task<ICollection<Link>> GetLinksAsync();

        Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter);

        Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter);

        Task<LinkLibrary> UpdateLibraryAsync(LinkLibrary library);
    }
}