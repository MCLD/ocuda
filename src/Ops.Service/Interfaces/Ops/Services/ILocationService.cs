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

        Task AddLocationMappingAsync(int productId, string importLocation, int locationId);

        Task DeleteAsync(int id);

        Task DeleteMappingAsync(int locationMapId);

        Task<Location> EditAlwaysOpenAsync(Location location);

        Task<Location> EditAsync(Location location);

        Task<IEnumerable<LocationProductMap>> GetAllLocationProductMapsAsync(int productId);

        Task<List<Location>> GetAllLocationsAsync();

        Task<IDictionary<int, string>> GetAllLocationsIdNameAsync();

        Task<IDictionary<int, string>> GetAllNamesIncludingDeletedAsync();

        Task<(double? Latitude, double? Longitude)> GetCoordinatesAsync(string address);

        Task<List<LocationDayGrouping>> GetFormattedWeeklyHoursAsync(int locationId);

        Task<Location> GetLocationByIdAsync(int locationId);

        Task<Location> GetLocationByStubAsync(string locationStub);

        Task<string> GetLocationLinkAsync(string placeId);

        Task<IDictionary<string, int>> GetLocationProductMapAsync(int productId);

        Task<ICollection<Location>> GetLocationsBySegment(int segmentId);

        Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string address);

        Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(LocationFilter filter);

        Task UndeleteAsync(int id);

        Task UpdateLocationMappingAsync(int locationMapId, string importLocation, int locationId);

        Task UploadLocationMapAsync();
    }
}