using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PageFeatureItemTextRepository 
        : GenericRepository<PromenadeContext, PageFeatureItemText>,
        IPageFeatureItemTextRepository
    {
        public PageFeatureItemTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PageFeatureItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageFeatureItemText> GetByIdsAsync(int featureItemId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureItemId == featureItemId && _.LanguageId == languageId
                    && !string.IsNullOrWhiteSpace(_.Filename))
                .SingleOrDefaultAsync();
        }
    }
}
