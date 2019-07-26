using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SocialCardRepository : GenericRepository<PromenadeContext, SocialCard, int>, ISocialCardRepository
    {
        public SocialCardRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SocialCardRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
