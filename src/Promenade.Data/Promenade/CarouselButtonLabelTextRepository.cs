using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CarouselButtonLabelTextRepository
        : GenericRepository<PromenadeContext, CarouselButtonLabelText>,
        ICarouselButtonLabelTextRepository
    {
        public CarouselButtonLabelTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselButtonLabelText> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselButtonLabelText> GetByIdsAsync(int labelId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselButtonLabelId == labelId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
