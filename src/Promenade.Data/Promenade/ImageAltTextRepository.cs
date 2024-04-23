using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ImageAltTextRepository : GenericRepository<PromenadeContext, LocationInteriorImageAltText>,
        IImageAltTextRepository
    {
        public ImageAltTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageAltTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationInteriorImageAltText> GetByImageIdAsync(int id, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationInteriorImageId == id && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }
    }
}