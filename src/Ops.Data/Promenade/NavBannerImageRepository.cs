using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavBannerImageRepository : GenericRepository<PromenadeContext, NavBannerImage>,
        INavBannerImageRepository
    {
        public NavBannerImageRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerImageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBannerImage> GetByNavBannerIdAsync(int navBannerId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerId == navBannerId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
