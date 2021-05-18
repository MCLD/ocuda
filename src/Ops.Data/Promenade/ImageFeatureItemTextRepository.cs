using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ImageFeatureItemTextRepository
        : GenericRepository<PromenadeContext, ImageFeatureItemText>, IImageFeatureItemTextRepository
    {
        public ImageFeatureItemTextRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageFeatureItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public void DetachEntity(ImageFeatureItemText itemText)
        {
            _context.Entry(itemText).State = EntityState.Detached;
        }

        public async Task<ICollection<ImageFeatureItemText>> GetAllForImageFeatureItemAsync(int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureItemId == itemId)
                .ToListAsync();
        }

        public async Task<ImageFeatureItemText>
                    GetByImageFeatureItemAndLanguageAsync(int imageFeatureItemId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ImageFeatureItemId == imageFeatureItemId
                    && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }
    }
}