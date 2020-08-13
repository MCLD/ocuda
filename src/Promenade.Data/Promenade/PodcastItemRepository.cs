using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.Extensions;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PodcastItemRepository
        : GenericRepository<PromenadeContext, PodcastItem>, IPodcastItemRepository
    {
        public PodcastItemRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PodcastItem> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PodcastItem> GetByStubAsync(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Stub == stub && !_.IsBlocked
                    && _.PublishDate <= _dateTimeProvider.Now)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<PodcastItem>> GetByPodcastIdAsync(int podcastId,
            bool showBlocked)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.PodcastId == podcastId && (!_.IsBlocked || showBlocked)
                    && _.PublishDate <= _dateTimeProvider.Now)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<PodcastItem>>> GetPaginatedListByPodcastIdAsync(
            int podcastId,
            PodcastFilter filter)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => _.PodcastId == podcastId && !_.IsDeleted && !_.IsBlocked
                    && _.PublishDate <= _dateTimeProvider.Now);

            var data = query;

            if (filter.SerialOrdering)
            {
                data = data.OrderBy(_ => _.Season)
                    .ThenBy(_ => _.Episode);
            }
            else
            {
                data = data.OrderByDescending(_ => _.PublishDate);
            }

            return new DataWithCount<ICollection<PodcastItem>>
            {
                Count = await query.CountAsync(),
                Data = await data
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
