using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaRepository
        : GenericRepository<PromenadeContext, Emedia>, IEmediaRepository
    {
        public EmediaRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Emedia> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<Emedia> GetIncludingGroupAsync(int id)
        {
            return await DbSet
                .Include(_ => _.Group)
                .Where(_ => _.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Emedia>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(
            int groupId, BaseFilter filter)
        {
            var query = DbSet.AsNoTracking().Where(_ => _.GroupId == groupId);

            return new DataWithCount<ICollection<Emedia>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
