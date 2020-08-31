using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CarouselTextRepository : GenericRepository<PromenadeContext, CarouselText>,
        ICarouselTextRepository
    {
        public CarouselTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselText> GetByCarouselAndLanguageAsync(int carouselId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselId == carouselId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<CarouselText>> GetForCarouselAsync(int carouselId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselId == carouselId)
                .ToListAsync();
        }
    }
}
