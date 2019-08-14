using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationGroupService
    {
        Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location);
        Task<LocationGroup> AddLocationGroupAsync(LocationGroup locationGroup);
        Task DeleteAsync(int id);
        Task<LocationGroup> GetLocationGroupByIdAsync(int locationgroupId);
        Task<LocationGroup> EditAsync(LocationGroup locationGroup);
    }
}
