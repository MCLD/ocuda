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

        public async Task<Page> GetPublishedByStubAndTypeAsync(string stub, PageType type,
            int languageId)
        {
            var pageHeaderId = _context.PageHeaders
                .AsNoTracking()
                .Where(_ => _.Stub == stub && _.Type == type)
                .Select(_ => _.Id);

            return await DbSet
                .AsNoTracking()
                .Where(_ => pageHeaderId.Contains(_.PageHeaderId)
                    && _.LanguageId == languageId
                    && _.IsPublished)
                .SingleOrDefaultAsync();
        }
    }
}
