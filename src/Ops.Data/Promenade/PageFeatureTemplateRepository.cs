using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageFeatureTemplateRepository
        : GenericRepository<PromenadeContext, PageFeatureTemplate>,
        IPageFeatureTemplateRepository
    {
        public PageFeatureTemplateRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageFeatureTemplateRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<PageFeatureTemplate>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<PageFeatureTemplate> GetForPageFeatureAsync(int featureId)
        {
            return await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Items.Any(_ => _.PageFeatureId == featureId))
                .Select(_ => _.PageHeader.LayoutFeatureTemplate)
                .SingleOrDefaultAsync();
        }
    }
}
