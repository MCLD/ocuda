using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class RenewCardRequestRepository
        : GenericRepository<PromenadeContext, RenewCardRequest>, IRenewCardRequestRepository
    {
        public RenewCardRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<RenewCardRequest> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(RenewCardRequest request)
        {
            await DbSet.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task<RenewCardRequest> GetPendingRequestAsync(int customerId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CustomerId == customerId && !_.IsDiscarded && !_.ProcessedAt.HasValue)
                .OrderByDescending(_ => _.SubmittedAt)
                .FirstOrDefaultAsync();
        }
    }
}