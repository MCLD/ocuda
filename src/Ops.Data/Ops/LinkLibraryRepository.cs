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
    public class LinkLibraryRepository
        : OpsRepository<OpsContext, LinkLibrary, int>, ILinkLibraryRepository
    {
        public LinkLibraryRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<LinkLibraryRepository> logger) : base(repositoryFacade, logger)
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

        public async Task<List<LinkLibrary>> GetLinkLibrariesBySectionIdAsync(int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToListAsync();
        }
    }
}
