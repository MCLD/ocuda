using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ImageFeatureItemTextRepository
        : GenericRepository<PromenadeContext, ImageFeatureItemText>, IImageFeatureItemTextRepository
    {
        public ImageFeatureItemTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageFeatureItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ImageFeatureItemText> GetByIdsAsync(int imageFeatureItemId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureItemId == imageFeatureItemId
                    && _.LanguageId == languageId
                    && !string.IsNullOrWhiteSpace(_.Filename))
                .SingleOrDefaultAsync();
        }
    }
}