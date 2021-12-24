using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavigationRepository 
        : GenericRepository<PromenadeContext, Navigation>, INavigationRepository
    {
        public NavigationRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavigationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Navigation> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<Navigation>> GetTopLevelNavigationsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.NavigationId.HasValue)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<ICollection<Navigation>> GetChildrenAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavigationId == id)
                .ToListAsync();
        }
    }
}
