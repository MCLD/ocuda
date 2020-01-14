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
    public class SegmentRepository : GenericRepository<PromenadeContext, Segment>, ISegmentRepository
    {
        public SegmentRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Segment> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<Segment>> GetAllActiveSegmentsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Segment>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public Segment FindSegmentByName(string name)
        {
            return DbSet.AsNoTracking()
                .Where(_ => _.Name == name)
                .FirstOrDefault();
        }

        public async Task<bool> IsDuplicateNameAsync(Segment segment)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != segment.Id && _.Name == segment.Name)
                .AnyAsync();
        }
    }
}
