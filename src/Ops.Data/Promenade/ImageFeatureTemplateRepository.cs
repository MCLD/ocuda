using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ImageFeatureTemplateRepository : GenericRepository<PromenadeContext, ImageFeatureTemplate>,
        IImageFeatureTemplateRepository
    {
        public ImageFeatureTemplateRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageFeatureTemplateRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<ImageFeatureTemplate>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<ImageFeatureTemplate> GetForImageFeatureAsync(int imageFeatureId)
        {
            var webslideTemplate = await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Items.Any(_ => _.WebslideId == imageFeatureId))
                .Select(_ => _.PageHeader.LayoutWebslideTemplate)
                .FirstOrDefaultAsync();

            return webslideTemplate ?? await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Items.Any(_ => _.PageFeatureId == imageFeatureId))
                .Select(_ => _.PageHeader.LayoutFeatureTemplate)
                .FirstOrDefaultAsync();
        }
    }
}