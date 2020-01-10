using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SocialCardRepository : GenericRepository<PromenadeContext, SocialCard>, ISocialCardRepository
    {
        public SocialCardRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SocialCardRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SocialCard> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }
    }
}
