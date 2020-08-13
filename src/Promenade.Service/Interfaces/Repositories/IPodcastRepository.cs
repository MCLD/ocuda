using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPodcastRepository : IGenericRepository<Podcast>
    {
        Task<Podcast> GetByStubAsync(string stub, bool showBlocked);
        Task<DataWithCount<ICollection<Podcast>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<PodcastDirectoryInfo>> GetDirectoryInfosByPodcastIdAsync(int id);
    }
}
