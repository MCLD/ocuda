using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PageLayoutTextRepository : GenericRepository<PromenadeContext, PageLayoutText>,
        IPageLayoutTextRepository
    {
        public PageLayoutTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PageLayoutText> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageLayoutText> GetByIdsAsync(int layoutId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageLayoutId == layoutId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
