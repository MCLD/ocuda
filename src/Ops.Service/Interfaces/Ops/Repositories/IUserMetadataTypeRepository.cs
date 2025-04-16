using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserMetadataTypeRepository : IOpsRepository<UserMetadataType, int>
    {
        Task<ICollection<UserMetadataType>> GetAllAsync();

        Task<DataWithCount<ICollection<UserMetadataType>>>
            GetPaginatedListAsync(BaseFilter filter);

        Task<bool> IsDuplicateAsync(UserMetadataType metadataType);
    }
}