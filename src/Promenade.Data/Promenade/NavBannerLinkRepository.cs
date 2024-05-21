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
    public class NavBannerLinkRepository : GenericRepository<PromenadeContext, NavBannerLink>,
        INavBannerLinkRepository
    {
        public NavBannerLinkRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerLink> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<NavBannerLink>> GetByNavBannerIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerId == id)
                .OrderBy(_ => _.Order)
                .ToListAsync();
        }
    }
}