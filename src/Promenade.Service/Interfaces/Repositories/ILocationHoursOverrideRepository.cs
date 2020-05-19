using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationHoursOverrideRepository : IGenericRepository<LocationHoursOverride>
    {
        Task<LocationHoursOverride> GetByDateAsync(int locationId, DateTime date);

        Task<ICollection<LocationHoursOverride>> GetBetweenDatesAsync(int locationId,
            DateTime startDate,
            DateTime endDate);

        public Task<string> GetClosureInformationAsync(DateTime date);
    }
}
