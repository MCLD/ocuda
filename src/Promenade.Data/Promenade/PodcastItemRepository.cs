using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PodcastItemRepository
        : GenericRepository<PromenadeContext, PodcastItem>, IPodcastItemRepository
    {
        public PodcastItemRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PodcastItem> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<PodcastItem>> GetByPodcastIdAsync(int podcastId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PodcastId == podcastId && !_.IsDeleted)
                .ToListAsync();
        }
    }
}
