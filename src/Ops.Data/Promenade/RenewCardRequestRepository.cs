using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class RenewCardRequestRepository :
        GenericRepository<PromenadeContext, RenewCardRequest>, IRenewCardRequestRepository
    {
        public RenewCardRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<RenewCardRequestRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task<RenewCardRequest> GetByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id && !_.IsDiscarded)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetCountAsync(bool? isProcessed)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDiscarded);

            if (isProcessed.HasValue)
            {
                query = query.Where(_ => _.ProcessedAt.HasValue == isProcessed);
            }

            return await query.CountAsync();
        }

        public async Task<CollectionWithCount<RenewCardRequest>> GetPaginatedAsync(
            RequestFilter filter)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDiscarded);

            if (filter?.IsProcessed.HasValue == true)
            {
                query = query.Where(_ => _.ProcessedAt.HasValue == filter.IsProcessed.Value);
            }

            if (filter?.IsProcessed == true)
            {
                query = query.OrderByDescending(_ => _.ProcessedAt);
            }
            else
            {
                query = query.OrderBy(_ => _.SubmittedAt);
            }

            return new CollectionWithCount<RenewCardRequest>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}