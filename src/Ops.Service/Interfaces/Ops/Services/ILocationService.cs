﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllLocationsAsync();
        Task<Location> GetLocationByStubAsync(string locationStub);
        Task<Location> AddLocationAsync(Location location);
        Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(BaseFilter filter);
        Task<Location> EditAsync(Location location);
        Task DeleteAsync(int id);
        Task<Location> GetLocationByIdAsync(int locationId);
        Task<List<string>> GetFormattedWeeklyHoursAsync(int locationId);
    }
}
