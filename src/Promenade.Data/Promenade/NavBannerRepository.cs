using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class NavBannerRepository : GenericRepository<PromenadeContext, NavBanner>,
        INavBannerRepository
    {
        public NavBannerRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBanner> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBanner> GetByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == id);
        }
    }
}