using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationHoursService
    {
        Task<List<LocationHours>> GetLocationHoursByIdAsync(int locationId);
        Task<LocationHours> AddLocationHoursAsync(LocationHours locationHours);
        Task<List<LocationHours>> EditAsync(List<LocationHours> locationHours);
        Task<ICollection<LocationHoursOverride>> GetLocationHoursOverrideByIdAsync(int locationId);
        Task<LocationHoursOverride> AddLocationHoursOverrideAsync(
            LocationHoursOverride hoursOverride);

        Task<LocationHoursOverride> EditLocationHoursOverrideAsync(
            LocationHoursOverride hoursOverride);

        Task DeleteLocationsHoursOverrideAsync(int id);
    }
}
