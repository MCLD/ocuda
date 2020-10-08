using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPodcastRepository : IGenericRepository<Podcast>
    {
        Task<Podcast> GetByIdAsync(int podcastId);
        Task<DataWithCount<ICollection<Podcast>>> GetPaginatedListAsync(BaseFilter filter);
    }
}
