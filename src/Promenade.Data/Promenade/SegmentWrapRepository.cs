using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SegmentWrapRepository
        : GenericRepository<PromenadeContext, SegmentWrap>, ISegmentWrapRepository
    {
        public SegmentWrapRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentWrapRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<SegmentWrap> GetActiveAsync(int segmentWrapId)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.Id == segmentWrapId && !_.IsDeleted)
                .SingleOrDefaultAsync();
        }
    }
}