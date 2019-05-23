using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IThumbnailRepository : IRepository<Thumbnail, int>
    {
        void RemoveByFileId(int fileId);
    }
}
