﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IUserMetadataTypeService
    {
        Task<ICollection<UserMetadataType>> GetAllAsync();
        Task<DataWithCount<ICollection<UserMetadataType>>> GetPaginatedListAsync(BaseFilter filter);
        Task<UserMetadataType> AddAsync(UserMetadataType metadataType);
        Task<UserMetadataType> EditAsync(UserMetadataType metadataType);
        Task DeleteAsync(int id);
    }
}
