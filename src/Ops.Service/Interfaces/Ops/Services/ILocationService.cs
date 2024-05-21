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
        int ExteriorImageHeight { get; }
        int ExteriorImageWidth { get; }
        int InteriorImageHeight { get; }
        int InteriorImageWidth { get; }

        Task AddAltTextRangeAsync(ICollection<LocationInteriorImageAltText> imageAltTexts);

        Task AddImageAltTextAsync(LocationInteriorImageAltText imageAltText);

        Task AddInteriorImageAsync(LocationInteriorImage locationInteriorImage);

        Task<Location> AddLocationAsync(Location location);

        Task AddLocationMappingAsync(int productId, string importLocation, int locationId);

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

        Task<string> GetExteriorImageFilePathAsync(string filename);

        Task<List<LocationDayGrouping>> GetFormattedWeeklyHoursAsync(int locationId);

        Task<LocationInteriorImageAltText> GetImageAltTextAsync(int imageId, int languageId);

        Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId);

        Task<string> GetInteriorImageFilePathAsync(string filename);

        Task<Location> GetLocationByCodeAsync(string locationCode);

        Task<Location> GetLocationByIdAsync(int locationId);

        Task<Location> GetLocationByStubAsync(string locationStub);

        Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId);

        Task<string> GetLocationLinkAsync(string placeId);

        Task<IDictionary<string, int>> GetLocationProductMapAsync(int productId);

        Task<ICollection<Location>> GetLocationsBySegment(int segmentId);

        Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string address);

        Task<string> GetMapImageFilePathAsync(string filename);

        Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(LocationFilter filter);

        Task<IDictionary<string, string>> GetSlugNameAsync();

        //Task<string> SaveImageToServerAsync(byte[] imageBytes,
        //    string requestedFilename,
        //    bool overwrite,
        //    string subDirectory);

        //Task<string> SaveImageToServerAsync(byte[] imageBytes,
        //    string requestedFilename,
        //    bool overwrite);

        Task UndeleteAsync(int id);

        Task UpdateExteriorImageAsync(IFormFile imageFile, string filename, string locationStub);

        Task UpdateImageAltTextAsync(int imageId, int languageId, string altText);

        Task UpdateInteriorImageAsync(LocationInteriorImage newInteriorImage,
            string filename,
            IFormFile imageFile);

        Task UpdateInteriorImageSortAsync(string slug, int interiorImageId, int increment);

        Task UpdateLocationMappingAsync(int locationMapId, string importLocation, int locationId);

        Task UpdateMapImageAsync(byte[] fileBytes, string filename, string locationStub);

        public Task UploadAddInteriorImageAsync(int locationId,
            string filename,
            IFormFile imageFile,
            IDictionary<int, string> altTexts);

        Task UploadLocationMapAsync(byte[] imageBytes, string fileName);
    }
}