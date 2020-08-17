using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Service
{
    public class PodcastService : BaseService<PodcastService>
    {
        private readonly IPodcastItemRepository _podcastItemRepository;
        private readonly IPodcastRepository _podcastRepository;

        public PodcastService(ILogger<PodcastService> logger,
            IDateTimeProvider dateTimeProvider,
            IPodcastItemRepository podcastItemRepository,
            IPodcastRepository podcastRepository)
            : base(logger, dateTimeProvider)
        {
            _podcastItemRepository = podcastItemRepository
                ?? throw new ArgumentNullException(nameof(podcastItemRepository));
            _podcastRepository = podcastRepository
                ?? throw new ArgumentNullException(nameof(podcastRepository));
        }

        public async Task<Podcast> GetByStubAsync(string stub, bool showBlocked = false)
        {
            return await _podcastRepository.GetByStubAsync(stub?.Trim(), showBlocked);
        }

        public async Task<DataWithCount<ICollection<Podcast>>> GetPaginatedListAsync(BaseFilter filter)
        {
            return await _podcastRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<PodcastDirectoryInfo>> GetDirectoryInfosByPodcastIdAsync(
            int id)
        {
            return await _podcastRepository.GetDirectoryInfosByPodcastIdAsync(id);
        }

        public async Task<PodcastItem> GetItemByStubAsync(string stub)
        {
            return await _podcastItemRepository.GetByStubAsync(stub?.Trim());
        }

        public async Task<ICollection<PodcastItem>> GetItemsByPodcastIdAsync(int id,
            bool showBlocked = false)
        {
            return await _podcastItemRepository.GetByPodcastIdAsync(id, showBlocked);
        }

        public async Task<DataWithCount<ICollection<PodcastItem>>> GetPaginatedItemsByPodcastIdAsync(
            int id,
            PodcastFilter filter)
        {
            return await _podcastItemRepository.GetPaginatedListByPodcastIdAsync(id, filter);
        }
    }
}
