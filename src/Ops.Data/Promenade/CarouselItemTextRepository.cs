using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CarouselItemTextRepository : GenericRepository<PromenadeContext, CarouselItemText>,
        ICarouselItemTextRepository
    {
        public CarouselItemTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselItemText> GetByCarouselItemAndLanguageAsync(int carouselItemId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItemId == carouselItemId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<CarouselItemText>> GetAllForCarouselAsync(int carouselId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItem.CarouselId == carouselId)
                .ToListAsync();
        }

        public async Task<ICollection<CarouselItemText>> GetAllForCarouselItemAsync(int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CarouselItemId == itemId)
                .ToListAsync();
        }
    }
}
