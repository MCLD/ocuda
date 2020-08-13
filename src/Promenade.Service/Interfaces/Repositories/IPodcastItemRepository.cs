using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPodcastItemRepository : IGenericRepository<PodcastItem>
    {
        Task<PodcastItem> GetByStubAsync(string stub);
        Task<ICollection<PodcastItem>> GetByPodcastIdAsync(int podcastId, bool showBlocked);
        Task<DataWithCount<ICollection<PodcastItem>>> GetPaginatedListByPodcastIdAsync(
            int podcastId, PodcastFilter filter);
    }
}
