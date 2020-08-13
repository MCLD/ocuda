using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageHeaderRepository
        : GenericRepository<PromenadeContext, PageHeader>, IPageHeaderRepository
    {
        public PageHeaderRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageHeaderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageHeader> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<DataWithCount<ICollection<PageHeader>>> GetPaginatedListAsync(
            PageFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.PageType.HasValue)
            {
                query = query.Where(_ => _.Type == filter.PageType.Value);
            }

            return new DataWithCount<ICollection<PageHeader>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.PageName)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<ICollection<string>> GetLanguagesByIdAsync(int id)
        {
            return await _context.Pages
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == id && _.IsPublished)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }

        public async Task<bool> StubInUseAsync(PageHeader header)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != header.Id
                    && _.Stub == header.Stub
                    && _.Type == header.Type)
                .AnyAsync();
        }
    }
}
