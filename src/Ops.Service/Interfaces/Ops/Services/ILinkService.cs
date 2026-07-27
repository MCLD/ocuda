using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILinkService
    {
        Task<LinkLibrary> CreateLibraryAsync(LinkLibrary library);

        Task UpdateLibrary(string sectionSlug, string slug, LinkLibrary library);

        Task<LinkLibrary> GetBySectionIdSlugAsync(int sectionId, string slug);

        Task<Link> CreateAsync(Link link);

        Task<int> GetLinkCountAsync(int linkLibraryId);

        Task DeleteAsync(int id);

        Task DeleteLibraryAsync(int id);

        Task<Link> EditAsync(Link link);

        Task<Link> GetByIdAsync(int id);

        Task<ICollection<LinkLibrary>> GetBySectionIdAsync(int sectionId);

        Task<LinkLibrary> GetLibraryByIdAsync(int id);

        Task<ICollection<Link>> GetLinkLibraryLinksAsync(int id);

        Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(LinksFilter filter);
    }
}
