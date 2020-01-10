using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageRepository
        : GenericRepository<PromenadeContext, Page>, IPageRepository
    {
        public PageRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<Page>> GetByHeaderIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == id)
                .ToListAsync();
        }

        public async Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.PageHeader)
                .Where(_ => _.PageHeaderId == headerId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
