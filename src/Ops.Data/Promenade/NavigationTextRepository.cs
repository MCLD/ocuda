using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavigationTextRepository
        : GenericRepository<PromenadeContext, NavigationText>, INavigationTextRepository
    {
        public NavigationTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavigationTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<NavigationText>> GetByNavigationIdsAsync(
            List<int> navigationIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => navigationIds.Contains(_.NavigationId))
                .ToListAsync();
        }
    }
}
