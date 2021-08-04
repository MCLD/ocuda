﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaTextRepository : IGenericRepository<EmediaText>
    {
        Task<ICollection<EmediaText>> GetAllForGroupAsync(int groupId);
    }
}