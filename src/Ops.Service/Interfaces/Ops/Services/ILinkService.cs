using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILinkService
    {
        Task<int> GetLinkCountAsync();
        Task<ICollection<Link>> GetLinksAsync();
        Task<Link> GetByIdAsync(int id);
        Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter);
        Task<Link> CreateAsync(int currentUserId, Link link);
        Task<Link> EditAsync(Link link);
        Task DeleteAsync(int id);
        Task<LinkLibrary> GetLibraryByIdAsync(int id);

        Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter);

        Task<LinkLibrary> CreateLibraryAsync(int currentUserId, LinkLibrary library);
        Task<LinkLibrary> EditLibraryAsync(LinkLibrary library);
        Task DeleteLibraryAsync(int id);
    }
}
