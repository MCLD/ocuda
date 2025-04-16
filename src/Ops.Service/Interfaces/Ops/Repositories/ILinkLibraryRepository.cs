using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILinkLibraryRepository : IOpsRepository<LinkLibrary, int>
    {
        Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedListAsync(BlogFilter filter);

        Task<List<LinkLibrary>> GetLinkLibrariesBySectionIdAsync(int sectionId);
    }
}