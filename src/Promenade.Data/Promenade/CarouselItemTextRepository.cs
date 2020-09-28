using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CarouselItemTextRepository : GenericRepository<PromenadeContext, CarouselItemText>,
        ICarouselItemTextRepository
    {
        public CarouselItemTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselItemText> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselItemText> GetByIdsAsync(int itemId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItemId == itemId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
