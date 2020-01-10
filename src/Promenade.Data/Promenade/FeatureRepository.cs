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

        public async Task<Feature> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }
    }
}
