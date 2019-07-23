using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class FeatureRepository : GenericRepository<Feature, int>, IFeatureRepository
    {
        public FeatureRepository(PromenadeContext context,
            ILogger<FeatureRepository> logger) : base(context, logger)
        {
        }
    }
}
