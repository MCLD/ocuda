using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavBannerLinkTextRepository
        : GenericRepository<PromenadeContext, NavBannerLinkText>,
        INavBannerLinkTextRepository
    {
        public NavBannerLinkTextRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerLinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBannerLinkText> FindAsync(int navBannerLinkId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerLinkId == navBannerLinkId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<NavBannerLinkText>> GetAllLanguageTextsAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerLinkId == id)
                .ToListAsync();
        }
    }
}