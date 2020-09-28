using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CarouselTemplateRepository : GenericRepository<PromenadeContext, CarouselTemplate>,
        ICarouselTemplateRepository
    {
        public CarouselTemplateRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselTemplate> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CarouselTemplate> GetTemplateForCarouselAsync(int carouselId)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.CarouselId == carouselId)
                .Select(_ => _.PageLayout.PageHeader.LayoutCarouselTemplate)
                .SingleOrDefaultAsync();
        }
    }
}
