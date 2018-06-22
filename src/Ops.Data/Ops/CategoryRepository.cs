using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class CategoryRepository : GenericRepository<Models.Category, int>, ICategoryRepository
    {
        public CategoryRepository(OpsContext context, ILogger<CategoryRepository> logger)
            :base(context, logger)
        {
        }
    }
}
