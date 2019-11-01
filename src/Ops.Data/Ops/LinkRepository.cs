using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Data;

namespace Ocuda.Ops.Data.Ops
{
    public class LinkRepository
        : GenericRepository<OpsContext, Link, int>, ILinkRepository
    {
        public LinkRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<LinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Link> GetLatestByLibraryIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LinkLibraryId == id)
                .OrderByDescending(_ => _.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.LinkLibraryId.HasValue)
            {
                query = query.Where(_ => _.LinkLibraryId == filter.LinkLibraryId.Value);
            }

            return new DataWithCount<ICollection<Link>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
        public async Task<List<Link>> GetFileLibraryFilesAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LinkLibraryId == id)
                .ToListAsync();
        }
    }
}
