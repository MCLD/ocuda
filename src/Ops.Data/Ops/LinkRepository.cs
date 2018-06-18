using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Data.Ops
{
    public class LinkRepository : GenericRepository<Models.Link, int>
    {
        public LinkRepository(OpsContext context, ILogger<LinkRepository> logger)
            : base(context, logger)
        {
        }
    }
}
