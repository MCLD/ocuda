using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class PostCategoryRepository : GenericRepository<OpsContext, PostCategory, int>, IPostCategoryRepository
    {
        public PostCategoryRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<PostCategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<PostCategory>> GetPostsBySectionIdAsync(int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToListAsync();
        }

        public PostCategory GetPostCategoryByStub(string stub)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.Stub.ToLower() == stub.ToLower())
                .FirstOrDefault();
        }

        public async Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedListAsync(
            BaseFilter filter, int sectionId)
        {
            return new DataWithCount<ICollection<PostCategory>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => _.SectionId == sectionId)
                    .CountAsync(),
                Data = await DbSet
                    .AsNoTracking()
                    .Where(_=>_.SectionId == sectionId)
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
