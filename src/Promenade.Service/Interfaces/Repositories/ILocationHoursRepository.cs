using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationHoursRepository : IGenericRepository<LocationHours, int>
    {
        Task<LocationHours> GetByDayOfWeek(int locationId, DateTime date);
        Task<ICollection<LocationHours>> GetWeeklyHoursAsync(int locationId);
    }
}
