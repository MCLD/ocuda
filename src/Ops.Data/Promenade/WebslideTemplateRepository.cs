using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class WebslideTemplateRepository : GenericRepository<PromenadeContext, WebslideTemplate>,
        IWebslideTemplateRepository
    {
        public WebslideTemplateRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<WebslideTemplateRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<WebslideTemplate>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<WebslideTemplate> GetForPageLayoutAsync(int pageLayoutId)
        {
            return await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Id == pageLayoutId)
                .Select(_ => _.PageHeader.LayoutWebslideTemplate)
                .SingleOrDefaultAsync();
        }

        public async Task<WebslideTemplate> GetForWebslideAsync(int webslideId)
        {
            return await _context.PageLayouts
                .AsNoTracking()
                .Where(_ => _.Items.Any(_ => _.WebslideId == webslideId))
                .Select(_ => _.PageHeader.LayoutWebslideTemplate)
                .SingleOrDefaultAsync();
        }
    }
}
