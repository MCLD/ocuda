using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class LinkRepository(
        ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<LinkRepository> logger)
        : OpsRepository<OpsContext, Link, int>(repositoryFacade, logger),
        ILinkRepository
    {
        public async Task<ICollection<Link>> GetLinkLibraryLinksAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LinkLibraryId == id)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<Link> GetLatestByLibraryIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LinkLibraryId == id)
                .OrderByDescending(_ => _.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(LinksFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.LinkLibrary?.Id != null)
            {
                query = query.Where(_ => _.LinkLibraryId == filter.LinkLibrary.Id);
            }

            return new DataWithCount<ICollection<Link>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ThenBy(_ => _.Url)
                    .ApplyPagination(filter)
                    .ToListAsync(),
            };
        }
    }
}
