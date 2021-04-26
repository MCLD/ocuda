using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class WebslideRepository : GenericRepository<PromenadeContext, Webslide>,
        IWebslideRepository
    {
        public WebslideRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<WebslideRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Webslide> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<Webslide> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageHeaderIdForWebslideAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.WebslideId == id)
                .Select(_ => _.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetPageLayoutIdForWebslideAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.WebslideId == id)
                .Select(_ => _.PageLayoutId)
                .SingleOrDefaultAsync();
        }
    }
}
