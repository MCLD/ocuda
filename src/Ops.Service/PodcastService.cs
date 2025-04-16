using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLayer;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class PodcastService : BaseService<PodcastService>, IPodcastService
    {
        private readonly IPermissionGroupPodcastItemRepository
            _permissionGroupPodcastItemRepository;

        private readonly IPodcastRepository _podcastRepository;
        private readonly IPodcastItemsRepository _podcastItemRepository;

        public PodcastService(ILogger<PodcastService> logger,
            IHttpContextAccessor httpContextAccessor,
            IPermissionGroupPodcastItemRepository permissionGroupPodcastItemRepository,
            IPodcastRepository podcastRepository,
            IPodcastItemsRepository podcastItemRepository) : base(logger, httpContextAccessor)
        {
            _permissionGroupPodcastItemRepository = permissionGroupPodcastItemRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupPodcastItemRepository));
            _podcastRepository = podcastRepository
                ?? throw new ArgumentNullException(nameof(podcastRepository));
            _podcastItemRepository = podcastItemRepository
                ?? throw new ArgumentNullException(nameof(podcastItemRepository));
        }

        public async Task<Podcast> GetByIdAsync(int podcastId)
        {
            var podcast = await _podcastRepository.GetByIdAsync(podcastId);
            podcast.EpisodeCount = await _podcastItemRepository.GetEpisodeCount(podcastId);
            var perms = await _permissionGroupPodcastItemRepository.GetByPodcastId(podcastId);
            podcast.PermissionGroupIds = perms.Select(_ => _.PermissionGroupId
                .ToString(CultureInfo.InvariantCulture));
            return podcast;
        }

        public async Task<DataWithCount<ICollection<PodcastItem>>>
            GetPaginatedEpisodeListAsync(int podcastId, BaseFilter filter)
        {
            return await _podcastItemRepository.GetPaginatedListAsync(podcastId, filter);
        }

        public async Task<DataWithCount<ICollection<Podcast>>>
            GetPaginatedListAsync(BaseFilter filter)
        {
            var podcasts = await _podcastRepository.GetPaginatedListAsync(filter);
            foreach (var podcast in podcasts.Data)
            {
                podcast.EpisodeCount = await _podcastItemRepository.GetEpisodeCount(podcast.Id);
                var perms = await _permissionGroupPodcastItemRepository.GetByPodcastId(podcast.Id);
                podcast.PermissionGroupIds = perms.Select(_ => _.PermissionGroupId
                    .ToString(CultureInfo.InvariantCulture));
            }
            return podcasts;
        }

        public async Task<PodcastItem> GetPodcastItemByIdAsync(int podcastItemId)
        {
            return await _podcastItemRepository.GetByIdAsync(podcastItemId);
        }

        public async Task<PodcastItem> UpdatePodcastItemAsync(PodcastItem podcastItem)
        {
            _podcastItemRepository.Update(podcastItem);
            await _podcastItemRepository.SaveAsync();
            return podcastItem;
        }

        public async Task AddPodcastItemAsync(PodcastItem podcastItem)
        {
            await _podcastItemRepository.AddAsync(podcastItem);
            await _podcastItemRepository.SaveAsync();
        }

        public async Task<bool> HasEpisodeAsync(int podcastId, int episodeNumber)
        {
            return await _podcastItemRepository.GetByPodcastEpisodeAsync(podcastId, episodeNumber);
        }

        public PodcastItem GetFileInfo(string path)
        {
            using var mpegFile = new MpegFile(path);
            return new PodcastItem
            {
                Duration = Convert.ToInt32(mpegFile.Duration.TotalSeconds),
                MediaSize = Convert.ToInt32(new System.IO.FileInfo(path).Length)
            };
        }

        public async Task<PodcastItem> GetEpisodeBySegmentIdAsync(int segmentId)
        {
            var episode = await _podcastItemRepository.GetUsingSegmentAsync(segmentId);
            if (episode != null)
            {
                episode.Podcast = await _podcastRepository.GetByIdAsync(episode.PodcastId);
            }
            return episode;
        }
    }
}