using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class WebslideItemTextRepository : GenericRepository<PromenadeContext, WebslideItemText>,
        IWebslideItemTextRepository
    {
        public WebslideItemTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<WebslideItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<WebslideItemText> GetByIdsAsync(int webslideItemId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideItemId == webslideItemId && _.LanguageId == languageId
                    && !string.IsNullOrWhiteSpace(_.Filename))
                .SingleOrDefaultAsync();
        }
    }
}
