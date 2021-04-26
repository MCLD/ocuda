using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class WebslideItemRepository : GenericRepository<PromenadeContext, WebslideItem>,
        IWebslideItemRepository
    {
        public WebslideItemRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<WebslideItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<WebslideItem>> GetActiveForWebslideAsync(int webslideId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideId == webslideId
                    && (!_.StartDate.HasValue || _.StartDate <= _dateTimeProvider.Now)
                    && (!_.EndDate.HasValue || _.EndDate >= _dateTimeProvider.Now))
                .OrderBy(_ => _.Order)
                .ToListAsync();
        }
    }
}
