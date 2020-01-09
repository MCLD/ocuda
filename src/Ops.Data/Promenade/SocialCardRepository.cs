using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class SocialCardRepository
        : GenericRepository<PromenadeContext, SocialCard>, ISocialCardRepository
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

        public async Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<SocialCard>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Title)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
