using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class NavBannerImageRepository : GenericRepository<PromenadeContext, NavBannerImage>,
        INavBannerImageRepository
    {
        public NavBannerImageRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerImage> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBannerImage> GetByNavBannerIdAsync(int navBannerId, int languageId)
        {
            return await DbSet
                .Where(_ => _.NavBannerId == navBannerId && _.LanguageId == languageId)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
    }
}
