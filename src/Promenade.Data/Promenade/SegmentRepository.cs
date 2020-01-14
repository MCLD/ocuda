using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SegmentRepository
    : GenericRepository<PromenadeContext, Segment>, ISegmentRepository
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

        public async Task<Segment> GetActiveAsync(int id)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.Id == id && _.IsActive
                    && (!_.StartDate.HasValue || _.StartDate >= _dateTimeProvider.Now)
                    && (!_.EndDate.HasValue || _.StartDate <= _dateTimeProvider.Now))
                .SingleOrDefaultAsync();
        }
    }
}
