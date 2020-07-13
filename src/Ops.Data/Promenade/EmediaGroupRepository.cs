using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaGroupRepository
        : GenericRepository<PromenadeContext, EmediaGroup>, IEmediaGroupRepository
    {
        public EmediaGroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<EmediaGroup>> GetUsingSegmentAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId)
                .ToListAsync();
        }
    }
}
