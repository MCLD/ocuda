using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class FileCategoryRepository(
        Repository<OpsContext> repositoryFacade,
        ILogger<FileCategoryRepository> logger)
        : GenericRepository<OpsContext, FileCategory>(repositoryFacade, logger),
        IFileCategoryRepository
    {
        public async Task<FileCategory> FindAsync(int fileId, int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FileId == fileId && _.CategoryId == categoryId)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync(IEnumerable<int> fileIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => fileIds.Contains(_.FileId))
                .Select(_ => _.Category)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetCategoryIdsAsync(int fileId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FileId == fileId)
                .Select(_ => _.CategoryId)
                .ToListAsync();
        }
    }
}
