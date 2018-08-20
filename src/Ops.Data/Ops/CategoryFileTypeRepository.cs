using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CategoryFileTypeRepository 
        : GenericRepository<CategoryFileType, int>, ICategoryFileTypeRepository
    {
        public CategoryFileTypeRepository(OpsContext context, ILogger<CategoryFileType> logger) 
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<int>> GetFileTypeIdsByCategoryIdAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .Select(_ => _.FileTypeId)
                .ToArrayAsync();
        }

        public void RemoveByCategoryId(int categoryId)
        {
            var elements = DbSet.Where(_ => _.CategoryId == categoryId);
            DbSet.RemoveRange(elements);
        }

        public async Task<CategoryFileType> GetCategoryAndFileTypesByCategoryId(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Category)
                .Include(_ => _.FileType)
                .Where(_ => _.CategoryId == categoryId)
                .SingleOrDefaultAsync();
        }
    }
}
