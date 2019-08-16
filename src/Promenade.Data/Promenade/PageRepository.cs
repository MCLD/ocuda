using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PageRepository
        : GenericRepository<PromenadeContext, Page, int>, IPageRepository
    {
        public PageRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Page> GetByStubAndTypeAsync(string stub, PageType type)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub && _.Type == type && _.IsPublished)
                .SingleOrDefaultAsync();
        }
    }
}
