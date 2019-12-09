using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageHeaderRepository
        : GenericRepository<PromenadeContext, PageHeader, int>, IPageHeaderRepository
    {
        public PageHeaderRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageHeaderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<DataWithCount<ICollection<PageHeader>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

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
