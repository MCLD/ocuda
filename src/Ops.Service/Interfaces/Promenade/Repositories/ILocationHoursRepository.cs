using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationHoursRepository : IGenericRepository<LocationHours>
    {
        Task<LocationHours> GetByIdsAsync(DayOfWeek dayOfWeek, int locationId);
        Task<List<LocationHours>> GetLocationHoursByLocationId(int locationId);
        Task<bool> IsDuplicateDayAsync(LocationHours locationHour);
        Task<ICollection<LocationHours>> GetWeeklyHoursAsync(int locationId);
    }
}
