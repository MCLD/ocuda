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
    public class EmployeeCardRequestRepository
        : GenericRepository<PromenadeContext, EmployeeCardRequest>, IEmployeeCardRequestRepository
    {
        public EmployeeCardRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<EmployeeCardRequestRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task<EmployeeCardRequest> GetByIdAsync(int Id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == Id)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetCountAsync(bool? isProcessed)
        {
            var query = DbSet.AsNoTracking();

            if (isProcessed.HasValue)
            {
                query = query.Where(_ => _.ProcessedAt.HasValue == isProcessed);
            }

            return await query.CountAsync();
        }

        public async Task<CollectionWithCount<EmployeeCardRequest>> GetPaginatedAsync(
            EmployeeCardFilter filter)
        {
            var query = DbSet.AsNoTracking();

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

            return new CollectionWithCount<EmployeeCardRequest>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
