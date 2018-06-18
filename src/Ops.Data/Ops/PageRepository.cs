using Microsoft.Extensions.Logging;
using Ocuda.Utility.Data;

namespace Ocuda.Ops.Data.Ops
{
    public class PageRepository : GenericRepository<Models.Page, int>
    {
        public PageRepository(OpsContext context, ILogger<PageRepository> logger)
            : base(context, logger)
        {
        }
    }
}
