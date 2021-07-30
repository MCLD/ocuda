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

        public async Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<EmediaGroup>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
