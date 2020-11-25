using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PageLayoutRepository : GenericRepository<PromenadeContext, PageLayout>,
        IPageLayoutRepository
    {
        public PageLayoutRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<PageLayout> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageLayout> GetIncludingChildrenAsync(int id)
        {
            return await DbSet
                .Where(_ => _.Id == id)
                .Include(_ => _.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetCurrentLayoutIdForHeaderAsync(int headerId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == headerId
                    && _.StartDate.HasValue
                    && _.StartDate < _dateTimeProvider.Now)
                .OrderByDescending(_ => _.StartDate)
                .Select(_ => _.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetPreviewLayoutIdAsync(int headerId, Guid previewId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == headerId
                    && _.PreviewId == previewId)
                .Select(_ => _.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime?> GetNextStartDate(int headerId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == headerId
                    && _.StartDate.HasValue
                    && _.StartDate >= _dateTimeProvider.Now)
                .OrderBy(_ => _.StartDate)
                .Select(_ => _.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}
