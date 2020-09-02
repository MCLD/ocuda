using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageLayoutTextRepository : GenericRepository<PromenadeContext, PageLayoutText>,
        IPageLayoutTextRepository
    {
        public PageLayoutTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageLayoutTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageLayoutText> GetByPageLayoutAndLanguageAsync(int layoutId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayoutId == layoutId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<PageLayoutText>> GetAllForHeaderAsync(int headerId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayout.PageHeaderId == headerId)
                .ToListAsync();
        }

        public async Task<ICollection<PageLayoutText>> GetAllForLayoutAsync(int layoutId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayoutId == layoutId)
                .ToListAsync();
        }
    }
}
