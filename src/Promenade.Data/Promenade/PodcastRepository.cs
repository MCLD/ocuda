using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PodcastRepository 
        : GenericRepository<PromenadeContext, Podcast>, IPodcastRepository
    {
        public PodcastRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<Podcast> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Podcast> GetByStubAsync(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub && !_.IsDeleted)
                .SingleOrDefaultAsync();
        }
    }
}
