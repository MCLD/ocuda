using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class FeatureRepository
        : GenericRepository<PromenadeContext, Feature>, IFeatureRepository
    {
        public FeatureRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<FeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int?> GetIdBySlugAsync(string slug)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == slug)
                .Select(_ => _.Id)
                .SingleOrDefaultAsync();
        }
    }
}