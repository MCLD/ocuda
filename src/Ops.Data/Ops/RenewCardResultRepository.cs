using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RenewCardResultRepository : OpsRepository<OpsContext, RenewCardResult, int>,
        IRenewCardResultRepository
    {
        public RenewCardResultRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<RenewCardResultRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<RenewCardResult> GetForRequestAsync(int requestId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.CreatedByUser)
                .Where(_ => _.RenewCardRequestId == requestId)
                .SingleOrDefaultAsync();
        }

        public async Task<RenewCardResponse.ResponseType> GetRequestResponseTypeAsync(
            int requestId)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.RenewCardRequestId == requestId)
                .Select(_ => _.RenewCardResponse.Type)
                .SingleOrDefaultAsync();
        }
    }
}
