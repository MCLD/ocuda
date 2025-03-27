using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CarouselButtonLabelRepository
        : GenericRepository<PromenadeContext, CarouselButtonLabel>, ICarouselButtonLabelRepository
    {
        public CarouselButtonLabelRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CarouselButtonLabelRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<CarouselButtonLabel>> GetActiveAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDisabled)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }
    }
}