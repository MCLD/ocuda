using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CarouselTextRepository : GenericRepository<PromenadeContext, CarouselText>,
        ICarouselTextRepository
    {
        public CarouselTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselText> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselText> GetByIdsAsync(int carouselId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselId == carouselId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
