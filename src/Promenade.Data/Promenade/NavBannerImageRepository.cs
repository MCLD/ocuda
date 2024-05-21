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

        public async Task<NavBannerImage> GetByNavBannerIdAsync(int id, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.NavBannerId == id && _.LanguageId == languageId);
        }
    }
}