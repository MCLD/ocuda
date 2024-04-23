using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationService
    {
        Task AddAltTextRangeAsync(ICollection<LocationInteriorImageAltText> imageAltTexts);

        Task AddImageAltTextAsync(LocationInteriorImageAltText imageAltText);

        Task AddInteriorImageAsync(LocationInteriorImage locationInteriorImage);

        Task<Location> AddLocationAsync(Location location);

        Task AddLocationMappingAsync(int productId, string importLocation, int locationId);

        Task<string> AssetPathToFullPath(string imagePath);

        Task DeleteAsync(int id);

        Task DeleteInteriorImageAsync(int imageId);

        Task DeleteMappingAsync(int locationMapId);

        Task<Location> EditAlwaysOpenAsync(Location location);

        Task<Location> EditAsync(Location location);

        Task<List<LocationInteriorImageAltText>> GetAllLanguageImageAltTextsAsync(int imageId);

        Task<IEnumerable<LocationProductMap>> GetAllLocationProductMapsAsync(int productId);

        Task<List<Location>> GetAllLocationsAsync();

        Task<IDictionary<int, string>> GetAllLocationsIdNameAsync();

        Task<IDictionary<int, string>> GetAllNamesIncludingDeletedAsync();

        Task<(double? Latitude, double? Longitude)> GetCoordinatesAsync(string address);

        Task<List<LocationDayGrouping>> GetFormattedWeeklyHoursAsync(int locationId);

        Task<LocationInteriorImageAltText> GetImageAltTextAsync(int imageId, int languageId);

        Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId);

        Task<Location> GetLocationByCodeAsync(string locationCode);

        Task<Location> GetLocationByIdAsync(int locationId);

        Task<Location> GetLocationByStubAsync(string locationStub);

        Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId);

        Task<string> GetLocationLinkAsync(string placeId);

        Task<IDictionary<string, int>> GetLocationProductMapAsync(int productId);

        Task<ICollection<Location>> GetLocationsBySegment(int segmentId);

        Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string address);

        Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(LocationFilter filter);

        Task<string> SaveImageToServerAsync(byte[] imageBytes, string fileName, string subDirectory);

        Task<string> SaveImageToServerAsync(byte[] imageBytes, string fileName);

        Task UndeleteAsync(int id);

        Task UpdateExteriorImage(IFormFile imageFile, string locationStub);

        Task UpdateInteriorImageAsync(LocationInteriorImage newInteriorImage, IFormFile imageFile);

        Task UpdateLocationMapPathAsync(string locationCode, string mapImagePath);

        Task UpdateLocationMappingAsync(int locationMapId, string importLocation, int locationId);

        Task UploadLocationMapAsync(byte[] imageBytes, string fileName);
    }
}