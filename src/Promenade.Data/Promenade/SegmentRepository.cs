using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SegmentRepository
    : GenericRepository<PromenadeContext, Segment, int>, ISegmentRepository
    {
        public SegmentRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Segment>> GetAllActiveSegments()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .ToListAsync();
        }
    }
}
