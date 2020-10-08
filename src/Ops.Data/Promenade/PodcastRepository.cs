using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class PodcastRepository
        : GenericRepository<PromenadeContext, Podcast>, IPodcastRepository
    {
        public PodcastRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PodcastRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Podcast> GetByIdAsync(int podcastId)
        {
            return await DbSet.Where(_ => _.Id == podcastId).AsNoTracking().SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Podcast>>>
            GetPaginatedListAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<Podcast>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Title)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
