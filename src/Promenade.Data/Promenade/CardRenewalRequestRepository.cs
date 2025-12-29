using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CardRenewalRequestRepository
        : GenericRepository<PromenadeContext, CardRenewalRequest>, ICardRenewalRequestRepository
    {
        public CardRenewalRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CardRenewalRequest> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(CardRenewalRequest request)
        {
            await DbSet.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task<CardRenewalRequest> GetPendingRequestAsync(int patronId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PatronId == patronId && !_.IsDiscarded && !_.ProcessedAt.HasValue)
                .OrderByDescending(_ => _.SubmittedAt)
                .FirstOrDefaultAsync();
        }
    }
}
