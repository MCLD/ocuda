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
    public class EmployeeCardRequestRepository :
        GenericRepository<PromenadeContext, EmployeeCardRequest>, IEmployeeCardRequestRepository
    {
        public EmployeeCardRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<EmployeeCardRequest> logger)
            : base(repositoryFacade, logger)
        {
        }
        public async Task<EmployeeCardRequest> GetByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .Include(_ => _.Department)
                .SingleOrDefaultAsync();
        }

        public async Task<CollectionWithCount<EmployeeCardRequest>> GetPaginatedAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new CollectionWithCount<EmployeeCardRequest>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.SubmittedAt)
                    .ApplyPagination(filter)
                    .Include(_ => _.Department)
                    .ToListAsync()
            };
        }
    }
}
