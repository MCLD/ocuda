using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ImageFeatureItemRepository : GenericRepository<PromenadeContext, ImageFeatureItem>,
        IImageFeatureItemRepository
    {
        public ImageFeatureItemRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageFeatureItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ImageFeatureItem> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ImageFeatureItem> GetByImageFeatureAndOrderAsync(int imageFeatureId,
            int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureId == imageFeatureId && _.Order == order)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<ImageFeatureItem>> GetByImageFeatureAsync(int imageFeatureId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureId == imageFeatureId)
                .ToListAsync();
        }

        public async Task<int?> GetMaxSortOrderForImageFeatureAsync(int imageFeatureId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureId == imageFeatureId)
                .MaxAsync(_ => (int?)_.Order);
        }

        public async Task<List<ImageFeatureItem>> GetImageFeatureSubsequentAsync(int imageFeatureId,
            int order)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureId == imageFeatureId && _.Order > order)
                .ToListAsync();
        }
    }
}