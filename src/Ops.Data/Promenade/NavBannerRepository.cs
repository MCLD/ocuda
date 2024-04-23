using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavBannerRepository : GenericRepository<PromenadeContext, NavBanner>,
        INavBannerRepository
    {
        public NavBannerRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBanner> GetByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageHeaderIdAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.NavBannerId == id)
                .Select(_ => _.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageLayoutIdAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.NavBannerId == id)
                .Select(_ => _.PageLayoutId)
                .SingleOrDefaultAsync();
        }
    }
}