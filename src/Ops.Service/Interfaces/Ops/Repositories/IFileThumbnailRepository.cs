using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileThumbnailRepository : IOpsRepository<FileThumbnail, int>
    {
        public Task<ICollection<FileThumbnail>> GetByFileIdsAsync(IEnumerable<int> fileIds);
    }
}
