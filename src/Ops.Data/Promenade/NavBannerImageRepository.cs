using System.Collections.Generic;
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

        public async Task<int> CountAsync(int languageId, string filename)
        {
            return await DbSet
                .AsNoTracking()
                .CountAsync(_ => _.Filename == filename && _.LanguageId == languageId);
        }

        public async Task<ICollection<NavBannerImage>> GetAllByNavBannerIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerId == id)
                .ToListAsync();
        }

        public async Task<NavBannerImage> GetByNavBannerIdAsync(int id, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavBannerId == id && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}