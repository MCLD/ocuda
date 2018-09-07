using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserMetadataTypeRepository : IRepository<UserMetadataType, int>
    {
        Task<ICollection<UserMetadataType>> GetAllAsync();
        Task<DataWithCount<ICollection<UserMetadataType>>> GetPaginatedListAsync(BaseFilter filter);
        Task<bool> IsDuplicateAsync(UserMetadataType metadataType);
    }
}
