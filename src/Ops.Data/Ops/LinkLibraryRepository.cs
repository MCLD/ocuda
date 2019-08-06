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

namespace Ocuda.Ops.Data.Ops
{
    public class LinkLibraryRepository
        : GenericRepository<LinkLibrary, int>, ILinkLibraryRepository
    {
        public LinkLibraryRepository(OpsContext context, ILogger<LinkLibraryRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedListAsync(
            BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<LinkLibrary>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
