using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class NavigationRepository
        : GenericRepository<PromenadeContext, Navigation>, INavigationRepository
    {
        public NavigationRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
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

        public async Task<ICollection<Navigation>> GetChildren(int parentNavigationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavigationId == parentNavigationId)
                .OrderBy(_ => _.Order)
                .ToListAsync();
        }
    }
}
