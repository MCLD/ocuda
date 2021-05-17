using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ImageFeatureRepository : GenericRepository<PromenadeContext, ImageFeature>,
        IImageFeatureRepository
    {
        public ImageFeatureRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageFeatureRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ImageFeature> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ImageFeature> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageHeaderIdForImageFeatureAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.WebslideId == id || _.PageFeatureId == id)
                .Select(_ => _.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetPageLayoutIdForImageFeatureAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.WebslideId == id || _.PageFeatureId == id)
                .Select(_ => _.PageLayoutId)
                .SingleOrDefaultAsync();
        }
    }
}
