using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class FeatureRepository
        : GenericRepository<PromenadeContext, Feature, int>, IFeatureRepository
    {
        public FeatureRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<FeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
