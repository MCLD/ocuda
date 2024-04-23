using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavBannerLinkRepository : GenericRepository<PromenadeContext, NavBannerLink>,
        INavBannerLinkRepository
    {
        public NavBannerLinkRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerLinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBannerLink> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == id)
                ?? throw new OcudaException($"Unable to find {nameof(NavBannerLink)} id {id}");
        }

        public async Task<ICollection<NavBannerLink>> GetLinksByNavBannerIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerId == id)
                .ToListAsync();
        }
    }
}