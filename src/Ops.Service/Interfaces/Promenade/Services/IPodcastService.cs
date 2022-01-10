using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IPodcastService
    {
        Task<DataWithCount<ICollection<Podcast>>> GetPaginatedListAsync(BaseFilter filter);
        Task<Podcast> GetByIdAsync(int podcastId);
        Task<DataWithCount<ICollection<PodcastItem>>> GetPaginatedEpisodeListAsync(int podcastId,
            BaseFilter filter);
        Task<PodcastItem> GetPodcastItemByIdAsync(int podcastItemId);
        Task<PodcastItem> UpdatePodcastItemAsync(PodcastItem podcastItem);
        Task AddPodcastItemAsync(PodcastItem podcastItem);
        Task<bool> HasEpisodeAsync(int podcastId, int episodeNumber);
        public PodcastItem GetFileInfo(string path);
        Task<PodcastItem> GetEpisodeBySegmentIdAsync(int segmentId);
    }
}
