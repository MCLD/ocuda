using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CarouselTemplateRepository : GenericRepository<PromenadeContext, CarouselTemplate>,
        ICarouselTemplateRepository
    {

        public CarouselTemplateRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselTemplateRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<CarouselTemplate>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.ButtonUrlLabel)
                .ToListAsync();
        }

        public async Task<CarouselTemplate> GetForPageLayoutAsync(int pageLayoutId)
        {
            return await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Id == pageLayoutId)
                .Select(_ => _.PageHeader.LayoutCarouselTemplate)
                .SingleOrDefaultAsync();
        }

        public async Task<CarouselTemplate> GetByCarouselItemAsync(int itemId)
        {
            var carouselId = _context.CarouselItems
                .AsNoTracking()
                .Where(_ => _.Id == itemId)
                .Select(_ => _.CarouselId);

            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.CarouselId.HasValue && carouselId.Contains(_.CarouselId.Value))
                .Select(_ => _.PageLayout.PageHeader.LayoutCarouselTemplate)
                .SingleOrDefaultAsync();
        }
    }
}
