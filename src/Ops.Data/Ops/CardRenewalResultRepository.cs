using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CardRenewalResultRepository : OpsRepository<OpsContext, CardRenewalResult, int>,
        ICardRenewalResultRepository
    {
        public CardRenewalResultRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CardRenewalResultRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CardRenewalResult> GetForRequestAsync(int requestId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.CreatedByUser)
                .Where(_ => _.CardRenewalRequestId == requestId)
                .SingleOrDefaultAsync();
        }
    }
}
