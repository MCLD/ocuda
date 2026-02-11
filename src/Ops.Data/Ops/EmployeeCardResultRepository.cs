using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class EmployeeCardResultRepository : OpsRepository<OpsContext, EmployeeCardResult, int>,
        IEmployeeCardResultRepository
    {
        public EmployeeCardResultRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<EmployeeCardResultRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CollectionWithCount<EmployeeCardResult>> GetPaginatedAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new CollectionWithCount<EmployeeCardResult>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.SubmittedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
