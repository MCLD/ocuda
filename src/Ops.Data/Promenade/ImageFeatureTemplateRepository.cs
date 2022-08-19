using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

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

        public async Task AssociateWithPageAsync(int imageFeatureId,
            int pageLayoutId,
            int imageFeatureTemplateId)
        {
            var pageItem = await _context
                .PageItems
                .Include(_ => _.PageLayout)
                .AsNoTracking()
                .Where(_ => _.PageFeatureId == imageFeatureId && _.PageLayoutId == pageLayoutId)
                .SingleOrDefaultAsync();

            if (pageItem == null)
            {
                throw new OcudaException("Cannot find page item to update.");
            }

            var pageHeader = await _context
                .PageHeaders
                .Where(_ => _.Id == pageItem.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();

            if (pageHeader == null)
            {
                throw new OcudaException("Cannot find page header to update.");
            }

            if (pageItem.PageFeatureId.HasValue)
            {
                pageHeader.LayoutFeatureTemplateId = imageFeatureTemplateId;
            }
            else if (pageItem.WebslideId.HasValue)
            {
                pageHeader.LayoutWebslideTemplateId = imageFeatureTemplateId;
            }
            else
            {
                throw new OcudaException("Unable to determine the type of image feature to update the page header.");
            }

            _context.Update(pageHeader);
            await _context.SaveChangesAsync();
        }

        public async Task<ImageFeatureTemplate> FindAsync(int imageFeatureId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == imageFeatureId)
                .SingleOrDefaultAsync();
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

        public async Task UnassignAndRemoveFeatureAsync(int imageFeatureTemplateId)
        {
            var headersUsing = _context
                .PageHeaders
                .Where(_ => _.LayoutFeatureTemplateId == imageFeatureTemplateId
                    || _.LayoutWebslideTemplateId == imageFeatureTemplateId);

            foreach (var header in headersUsing)
            {
                if (header.LayoutFeatureTemplateId == imageFeatureTemplateId)
                {
                    header.LayoutFeatureTemplateId = null;
                }
                if (header.LayoutWebslideTemplateId == imageFeatureTemplateId)
                {
                    header.LayoutWebslideTemplateId = null;
                }
                _context.Update(header);
            }

            var item = DbSet.Where(_ => _.Id == imageFeatureTemplateId).Single();
            DbSet.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}
