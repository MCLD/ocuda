using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserSyncLocationRepository : IOpsRepository<UserSyncLocation, int>
    {
        public Task<ICollection<UserSyncLocation>> GetAllAsync();
    }
}