using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationHoursOverrideRepository : IGenericRepository<LocationHoursOverride>
    {
        Task<LocationHoursOverride> FindAsync(int id);
        Task<ICollection<LocationHoursOverride>> GetByLocationIdAsync(int locationId);
        Task<ICollection<LocationHoursOverride>> GetConflictingOverrideDatesAsync(
            LocationHoursOverride hoursOverride);
    }
}
