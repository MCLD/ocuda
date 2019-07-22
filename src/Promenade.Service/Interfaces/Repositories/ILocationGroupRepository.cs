using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationGroupRepository : IRepository<LocationGroup, int>
    {
        Task<List<LocationGroup>> GetGroupByLocationIdAsync(int locationId);
        Task<List<LocationGroup>> GetLocationsByGroupIdAsync(int groupId);
    }
}

