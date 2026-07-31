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
    public class LinkLibraryRepository(
        ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<LinkLibraryRepository> logger)
        : OpsRepository<OpsContext, LinkLibrary, int>(repositoryFacade, logger),
        ILinkLibraryRepository
    {
        public async Task<ICollection<LinkLibrary>> GetBySectionAsync(
            int sectionId,
            bool? isFeatured)
        {
            var libraries = DbSet.AsNoTracking().Where(_ => _.SectionId == sectionId);

            if (isFeatured.HasValue)
            {
                libraries = libraries.Where(_ => _.IsFeatured == isFeatured.Value);
            }

            return await libraries.OrderBy(_ => _.Name).ToListAsync();
        }

        public async Task<LinkLibrary> GetBySectionIdSlugAsync(int sectionId, string slug)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.SectionId == sectionId && _.Slug == slug);
        }

        public async Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedListAsync(
            BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId.Value);
            }

            return new DataWithCount<ICollection<LinkLibrary>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync(),
            };
        }
    }
}
