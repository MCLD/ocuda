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
    public class PodcastRepository
        : GenericRepository<PromenadeContext, Podcast>, IPodcastRepository
    {
        public PodcastRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<Podcast> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Podcast> GetByStubAsync(string stub, bool showBlocked)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Stub == stub && (!_.IsBlocked || showBlocked)
                    && _.PodcastItems.Any(pi => !pi.IsDeleted && (!pi.IsBlocked || showBlocked)
                        && pi.PublishDate <= _dateTimeProvider.Now))
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Podcast>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && !_.IsBlocked
                    && _.PodcastItems.Any(pi => !pi.IsDeleted && !pi.IsBlocked
                        && pi.PublishDate <= _dateTimeProvider.Now));

            return new DataWithCount<ICollection<Podcast>>
            {
                Count = await query.CountAsync(),
                Data = await query
                .OrderBy(_ => _.Title)
                .ApplyPagination(filter)
                .ToListAsync()
            };
        }

        public async Task<ICollection<PodcastDirectoryInfo>> GetDirectoryInfosByPodcastIdAsync(
            int id)
        {
            return await _context.PodcastDirectoryInfos
                .Include(_ => _.PodcastDirectory)
                .AsNoTracking()
                .Where(_ => _.PodcastId == id)
                .OrderBy(_ => _.PodcastDirectory.Name)
                .ToListAsync();
        }
    }
}
