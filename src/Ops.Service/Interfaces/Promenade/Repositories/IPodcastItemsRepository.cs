using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPodcastItemsRepository : IGenericRepository<PodcastItem>
    {
        Task<int> GetEpisodeCount(int podcastId);
        Task<DataWithCount<ICollection<PodcastItem>>>
            GetPaginatedListAsync(int podcastId, BaseFilter filter);
        Task<PodcastItem> GetByIdAsync(int podcastItemId);
        Task<bool> GetByPodcastEpisodeAsync(int podcastId, int episodeNumber);
    }
}
