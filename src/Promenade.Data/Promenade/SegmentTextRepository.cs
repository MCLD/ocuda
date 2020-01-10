using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SegmentTextRepository
    : GenericRepository<PromenadeContext, SegmentText>, ISegmentTextRepository
    {
        public SegmentTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SegmentText> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public SegmentText GetSegmentTextBySegmentAndLanguageId(int segmentId, int languageId)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId && _.LanguageId == languageId)
                .FirstOrDefault();
        }
    }
}