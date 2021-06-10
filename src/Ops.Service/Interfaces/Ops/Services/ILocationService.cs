using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationService
    {
        Task<Location> AddLocationAsync(Location location);

        Task DeleteAsync(int id);

        Task<Location> EditAlwaysOpenAsync(Location location);

        Task<Location> EditAsync(Location location);

        Task<List<Location>> GetAllLocationsAsync();

        Task<(double? Latitude, double? Longitude)> GetCoordinatesAsync(string address);

        Task<List<LocationDayGrouping>> GetFormattedWeeklyHoursAsync(int locationId);

        Task<Location> GetLocationByIdAsync(int locationId);

        Task<Location> GetLocationByStubAsync(string locationStub);

        Task<string> GetLocationLinkAsync(string placeId);

        Task<ICollection<Location>> GetLocationsBySegment(int segmentId);

        Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string address);

        Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(BaseFilter filter);
    }
}