using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class LinkRepository 
        : GenericRepository<Models.Link, int>, ILinkRepository
    {
        public LinkRepository(OpsContext context, ILogger<LinkRepository> logger)
            : base(context, logger)
        {
        }
    }
}
