using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class PodcastItemsRepository
        : GenericRepository<PromenadeContext, PodcastItem>, IPodcastItemsRepository
    {
        public PodcastItemsRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PodcastItemsRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PodcastItem> GetByIdAsync(int podcastItemId)
        {
            return await DbSet
                .Include(_ => _.Podcast)
                .Where(_ => _.Id == podcastItemId)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<bool> GetByPodcastEpisodeAsync(int podcastId, int episodeNumber)
        {
            return await DbSet
                .Where(_ => _.PodcastId == podcastId && _.Episode == episodeNumber)
                .AsNoTracking()
                .AnyAsync();
        }

        public async Task<int> GetEpisodeCount(int podcastId)
        {
            return await DbSet
                .Where(_ => _.PodcastId == podcastId)
                .AsNoTracking()
                .CountAsync();
        }

        public async Task<DataWithCount<ICollection<PodcastItem>>>
            GetPaginatedListAsync(int podcastId, BaseFilter filter)
        {
            var query = DbSet.Where(_ => _.PodcastId == podcastId).AsNoTracking();
            return new DataWithCount<ICollection<PodcastItem>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.Episode)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
