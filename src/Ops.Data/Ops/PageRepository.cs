using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class PageRepository 
        : GenericRepository<Models.Page, int>, IPageRepository
    {
        public PageRepository(OpsContext context, ILogger<PageRepository> logger)
            : base(context, logger)
        {
        }
    }
}
