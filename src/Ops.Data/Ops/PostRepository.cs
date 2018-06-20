using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class PostRepository 
        : GenericRepository<Models.Post, int>, IPostRepository
    {
        public PostRepository(OpsContext context, ILogger<PostRepository> logger) 
            : base(context, logger)
        {
        }
    }
}
