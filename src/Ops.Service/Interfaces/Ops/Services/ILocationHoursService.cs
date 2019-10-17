using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationHoursService
    {
        Task<List<LocationHours>> GetLocationHoursByIdAsync(int locationId);
        Task<LocationHours> AddLocationHoursAsync(LocationHours locationHours);
        Task<List<LocationHours>> EditAsync(List<LocationHours> locationHours);
        Task DeleteAsync(int id);
    }
}
