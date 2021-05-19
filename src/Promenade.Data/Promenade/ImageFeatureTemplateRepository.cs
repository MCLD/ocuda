using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ImageFeatureTemplateRepository
        : GenericRepository<PromenadeContext, ImageFeatureTemplate>,
        IImageFeatureTemplateRepository
    {
        public ImageFeatureTemplateRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageFeatureTemplateRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ImageFeatureTemplate> GetForPageLayoutAsync(int pageLayoutId)
        {
            return await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Id == pageLayoutId)
                .Select(_ => _.PageHeader.LayoutFeatureTemplate)
                .SingleOrDefaultAsync();
        }
    }
}