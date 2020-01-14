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

        public async Task<SegmentText> GetByIdsAsync(int languageId, int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LanguageId == languageId && _.SegmentId == segmentId)
                .SingleOrDefaultAsync();
        }
    }
}