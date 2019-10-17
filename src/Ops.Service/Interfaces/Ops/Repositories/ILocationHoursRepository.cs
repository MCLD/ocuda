using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILocationHoursRepository : IRepository<LocationHours, int>
    {
        Task<List<LocationHours>> GetLocationHoursByLocationId(int locationId);
        Task<bool> IsDuplicateDayAsync(LocationHours locationHour);
        Task<ICollection<LocationHours>> GetWeeklyHoursAsync(int locationId);
    }
}
