using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class FileRepository 
        : GenericRepository<Models.File, int>, IFileRepository
    {
        public FileRepository(OpsContext context, ILogger<FileRepository> logger)
            : base(context, logger)
        {
        }
    }
}
