using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavBannerLinkRepository : GenericRepository<PromenadeContext, NavBannerLink>,
        INavBannerLinkRepository
    {
        public NavBannerLinkRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerLinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<NavBannerLink>> GetLinksByNavBannerIdAsync(int navBannerId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerId == navBannerId)
                .ToListAsync();
        }
    }
}
