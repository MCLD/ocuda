using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Data.Ops
{
    public class PostRepository : GenericRepository<Models.Post, int>
    {
        public PostRepository(OpsContext context, ILogger<PostRepository> logger) 
            : base(context, logger)
        {
        }
    }
}
