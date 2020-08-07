using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

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

        public async Task<Podcast> GetPodcastByStubAsync(string stub)
        {
            return await _podcastRepository.GetByStubAsync(stub?.Trim());
        }

        public async Task<ICollection<PodcastItem>> GetItemsByPodcastIdAsync(int id)
        {
            return await _podcastItemRepository.GetByPodcastIdAsync(id);
        }
    }
}
