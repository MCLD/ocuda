using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class LocationService : BaseService<LocationService>, ILocationService
    {
        private const string ndash = "\u2013";
        private readonly IGoogleClient _googleClient;
        private readonly IImageAltTextRepository _imageAltTextRepository;
        private readonly IImageService _imageService;
        private readonly ILocationFeatureRepository _locationFeatureRepository;
        private readonly ILocationGroupRepository _locationGroupRepository;
        private readonly ILocationHoursOverrideRepository _locationHoursOverrideRepository;
        private readonly ILocationHoursRepository _locationHoursRepository;
        private readonly ILocationInteriorImageRepository _locationInteriorImageRepository;
        private readonly ILocationProductMapRepository _locationProductMapRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IRosterDivisionRepository _rosterDivisionRepository;
        private readonly IRosterLocationRepository _rosterLocationRepository;
        private readonly ISegmentService _segmentService;
        private readonly ISiteSettingService _siteSettingService;

        public LocationService(IGoogleClient googleClient,
            IImageService imageService,
            IImageAltTextRepository imageAltTextRepository,
            ILocationInteriorImageRepository locationInteriorImageRepository,
            ILocationFeatureRepository locationFeatureRepository,
            ILocationGroupRepository locationGroupRepository,
            IHttpContextAccessor httpContextAccessor,
            ILocationHoursOverrideRepository locationHoursOverrideRepository,
            ILocationHoursRepository locationHoursRepository,
            ILocationProductMapRepository locationProductMapRepository,
            ILocationRepository locationRepository,
            ILogger<LocationService> logger,
            IRosterDivisionRepository rosterDivisionRepository,
            IRosterLocationRepository rosterLocationRepository,
            ISegmentService segmentService,
            ISiteSettingService siteSettingService)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(googleClient);
            ArgumentNullException.ThrowIfNull(imageService);
            ArgumentNullException.ThrowIfNull(imageAltTextRepository);
            ArgumentNullException.ThrowIfNull(locationInteriorImageRepository);
            ArgumentNullException.ThrowIfNull(locationFeatureRepository);
            ArgumentNullException.ThrowIfNull(locationGroupRepository);
            ArgumentNullException.ThrowIfNull(locationHoursOverrideRepository);
            ArgumentNullException.ThrowIfNull(locationHoursRepository);
            ArgumentNullException.ThrowIfNull(locationProductMapRepository);
            ArgumentNullException.ThrowIfNull(locationRepository);
            ArgumentNullException.ThrowIfNull(rosterDivisionRepository);
            ArgumentNullException.ThrowIfNull(rosterLocationRepository);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(siteSettingService);

            _googleClient = googleClient;
            _imageService = imageService;
            _imageAltTextRepository = imageAltTextRepository;
            _locationInteriorImageRepository = locationInteriorImageRepository;
            _locationFeatureRepository = locationFeatureRepository;
            _locationGroupRepository = locationGroupRepository;
            _locationHoursOverrideRepository = locationHoursOverrideRepository;
            _locationHoursRepository = locationHoursRepository;
            _locationProductMapRepository = locationProductMapRepository;
            _locationRepository = locationRepository;
            _rosterDivisionRepository = rosterDivisionRepository;
            _rosterLocationRepository = rosterLocationRepository;
            _segmentService = segmentService;
            _siteSettingService = siteSettingService;

            ExteriorImageHeight = 1024;
            ExteriorImageWidth = 1600;
            InteriorImageHeight = 1036;
            InteriorImageWidth = 1600;
        }

        public int ExteriorImageHeight { get; set; }
        public int ExteriorImageWidth { get; set; }

        public int InteriorImageHeight { get; set; }

        public int InteriorImageWidth { get; set; }

        public async Task AddAltTextRangeAsync(ICollection<LocationInteriorImageAltText> imageAltTexts)
        {
            ArgumentNullException.ThrowIfNull(imageAltTexts);

            foreach (var altText in imageAltTexts)
            {
                altText.AltText = altText.AltText?.Trim();
            }

            await _imageAltTextRepository.AddRangeAsync(imageAltTexts);
            await _imageAltTextRepository.SaveAsync();
        }

        public async Task AddImageAltTextAsync(LocationInteriorImageAltText imageAltText)
        {
            await _imageAltTextRepository.AddAsync(imageAltText);
            await _locationInteriorImageRepository.SaveAsync();
        }

        public async Task AddInteriorImageAsync(LocationInteriorImage locationInteriorImage)
        {
            ArgumentNullException.ThrowIfNull(locationInteriorImage);

            await _locationInteriorImageRepository.AddAsync(locationInteriorImage);
            await _locationInteriorImageRepository.SaveAsync();

            await FixInteriorImageSortOrder(locationInteriorImage.LocationId);
        }

        public async Task<Location> AddLocationAsync(Location location)
        {
            ArgumentNullException.ThrowIfNull(location);

            location.AddressType = location.AddressType?.Trim();
            location.AdministrativeArea = location.AdministrativeArea?.Trim();
            location.AreaServedName = location.AreaServedName?.Trim();
            location.AreaServedType = location.AreaServedType?.Trim();
            location.ContactType = location.ContactType?.Trim();
            location.Email = location.Email?.Trim();
            location.Name = location.Name?.Trim();
            location.ParentOrganization = location.ParentOrganization?.Trim();
            location.PriceRange = location.PriceRange?.Trim();
            location.Type = location.Type?.Trim();
            await ValidateAsync(location);

            await _locationRepository.AddAsync(location);
            await _locationRepository.SaveAsync();

            return location;
        }

        public async Task AddLocationMappingAsync(int productId, string importLocation, int locationId)
        {
            try
            {
                await _locationProductMapRepository.AddAsync(new LocationProductMap
                {
                    ImportLocation = importLocation,
                    LocationId = locationId,
                    ProductId = productId
                });
                await _locationProductMapRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new OcudaException(ex.Message, ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var location = await _locationRepository.FindAsync(id);

            var locationGroups = await _locationGroupRepository.GetLocationGroupsByLocationAsync(location);
            _locationGroupRepository.RemoveRange(locationGroups);
            var locationFeatures = await _locationFeatureRepository.GetLocationFeaturesByLocationId(id);
            _locationFeatureRepository.RemoveRange(locationFeatures);

            _locationProductMapRepository.RemoveForLocation(location.Id);

            var locationHours = await _locationHoursRepository.GetLocationHoursByLocationId(id);
            _locationHoursRepository.RemoveRange(locationHours);

            var hourOverrides = await _locationHoursOverrideRepository.GetByLocationIdAsync(location.Id);
            _locationHoursOverrideRepository.RemoveRange(hourOverrides);

            var rosterDivisions = await _rosterDivisionRepository.GetAllAsync();
            var mappedRosterDivisions = rosterDivisions.Where(_ => _.MapToLocationId == location.Id);
            if (mappedRosterDivisions != null && mappedRosterDivisions?.Count() > 0)
            {
                _rosterDivisionRepository.RemoveRange(mappedRosterDivisions.ToArray());
            }

            var rosterLocations = await _rosterLocationRepository.GetAllAsync();
            var mappedRosterLocations = rosterLocations.Where(_ => _.MapToLocationId == location.Id);
            if (mappedRosterLocations != null && mappedRosterLocations?.Count() > 0)
            {
                _rosterLocationRepository.RemoveRange(mappedRosterLocations.ToArray());
            }

            location.IsDeleted = true;
            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();
        }

        public async Task DeleteInteriorImageAsync(int imageId)
        {
            var image = await _locationInteriorImageRepository.GetInteriorImageByIdAsync(imageId);
            var imageAltTexts = await _imageAltTextRepository
                .GetAllLanguageImageAltTextsAsync(imageId);

            var locationId = image.LocationId;

            var imagePath = await GetInteriorImageFilePathAsync(image.ImagePath);

            _locationInteriorImageRepository.Remove(image);
            _imageAltTextRepository.RemoveRange(imageAltTexts);

            await _imageAltTextRepository.SaveAsync();

            File.Delete(imagePath);
            _logger.LogInformation("Interior image {Filename} deleted", image.ImagePath);
            await FixInteriorImageSortOrder(locationId);
        }

        public async Task DeleteMappingAsync(int locationMapId)
        {
            var mapping = await _locationProductMapRepository.FindAsync(locationMapId);
            _locationProductMapRepository.Remove(mapping);
            await _locationProductMapRepository.SaveAsync();
        }

        public async Task<Location> EditAlwaysOpenAsync(Location location)
        {
            ArgumentNullException.ThrowIfNull(location);

            var currentLocation = await _locationRepository.FindAsync(location.Id);
            currentLocation.IsAlwaysOpen = location.IsAlwaysOpen;

            await ValidateAsync(currentLocation);

            _locationRepository.Update(currentLocation);
            await _locationRepository.SaveAsync();
            return currentLocation;
        }

        public async Task<Location> EditAsync(Location location)
        {
            ArgumentNullException.ThrowIfNull(location);

            await ValidateAsync(location);

            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();
            return location;
        }

        public async Task<List<LocationInteriorImageAltText>> GetAllLanguageImageAltTextsAsync(int imageId)
        {
            return await _imageAltTextRepository.GetAllLanguageImageAltTextsAsync(imageId);
        }

        public async Task<IEnumerable<LocationProductMap>> GetAllLocationProductMapsAsync(int productId)
        {
            return await _locationProductMapRepository.GetByProductAsync(productId);
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _locationRepository.GetAllLocationsAsync();
        }

        public async Task<IDictionary<int, string>> GetAllLocationsIdNameAsync()
        {
            return await _locationRepository.GetAllLocationsIdNameAsync();
        }

        public async Task<IDictionary<int, string>> GetAllNamesIncludingDeletedAsync()
        {
            return await _locationRepository.GetAllNamesIncludingDeletedAsync();
        }

        public async Task<(double? Latitude, double? Longitude)>
            GetCoordinatesAsync(string address)
        {
            return await _googleClient.GeocodeAsync(address);
        }

        public async Task<string> GetExteriorImageFilePathAsync(string filename)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            return Path.Combine(promBasePath,
                UriPaths.Images,
                UriPaths.Locations,
                filename);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Normalize AM/PM to lower-case")]
        public async Task<List<LocationDayGrouping>> GetFormattedWeeklyHoursAsync(int locationId)
        {
            var location = await _locationRepository.FindAsync(locationId);
            if (location.IsAlwaysOpen)
            {
                return null;
            }

            var weeklyHours = await _locationHoursRepository
                .GetLocationHoursByLocationId(locationId);
            // Order weeklyHours to start on Monday
            weeklyHours = weeklyHours.OrderBy(_ => ((int)_.DayOfWeek + 1) % 8).ToList();

            var dayGroupings = new List<(List<DayOfWeek> DaysOfWeek, DateTime OpenTime, DateTime CloseTime)>();
            var closedDays = new List<DayOfWeek>();
            foreach (var day in weeklyHours)
            {
                if (day.Open)
                {
                    var (DaysOfWeek, OpenTime, CloseTime) = dayGroupings.LastOrDefault();
                    if (dayGroupings.Count > 0 && OpenTime == day.OpenTime
                        && CloseTime == day.CloseTime)
                    {
                        DaysOfWeek.Add(day.DayOfWeek);
                    }
                    else
                    {
                        dayGroupings.Add((
                            DaysOfWeek: new List<DayOfWeek> { day.DayOfWeek },
                            OpenTime: day.OpenTime.Value,
                            CloseTime: day.CloseTime.Value));
                    }
                }
                else
                {
                    closedDays.Add(day.DayOfWeek);
                }
            }

            var formattedDayGroupings = new List<LocationDayGrouping>();
            foreach (var (DaysOfWeek, OpenTime, CloseTime) in dayGroupings)
            {
                var days = GetFormattedDayGroupings(DaysOfWeek);

                var openTimeString = new StringBuilder(OpenTime.ToString("%h",
                    CultureInfo.InvariantCulture));

                if (OpenTime.Minute != 0)
                {
                    openTimeString.Append(OpenTime.ToString(":mm", CultureInfo.InvariantCulture));
                }

                openTimeString.Append(OpenTime
                    .ToString(" tt", CultureInfo.InvariantCulture)
                    .ToLowerInvariant());

                var closeTimeString = new StringBuilder(CloseTime.ToString("%h",
                    CultureInfo.InvariantCulture));

                if (CloseTime.Minute != 0)
                {
                    closeTimeString.Append(CloseTime.ToString(":mm",
                        CultureInfo.InvariantCulture));
                }
                closeTimeString.Append(CloseTime
                    .ToString(" tt", CultureInfo.InvariantCulture)
                    .ToLowerInvariant());

                formattedDayGroupings.Add(new LocationDayGrouping
                {
                    Days = days,
                    Time = $"{openTimeString} {ndash} {closeTimeString}"
                });
            }

            if (closedDays.Count > 0)
            {
                var formattedClosedDays = GetFormattedDayGroupings(closedDays);
                formattedDayGroupings.Add(new LocationDayGrouping
                {
                    Days = formattedClosedDays,
                    Time = "Closed"
                });
            }

            return formattedDayGroupings;
        }

        public async Task<LocationInteriorImageAltText> GetImageAltTextAsync(int imageId, int languageId)
        {
            return await _imageAltTextRepository.GetImageAltTextAsync(imageId, languageId);
        }

        public async Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId)
        {
            return await _locationInteriorImageRepository.GetInteriorImageByIdAsync(imageId);
        }

        public async Task<string> GetInteriorImageFilePathAsync(string filename)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            return Path.Combine(promBasePath,
                UriPaths.Images,
                UriPaths.Locations,
                UriPaths.Interior,
                filename);
        }

        public async Task<Location> GetLocationByCodeAsync(string locationCode)
        {
            var location = await _locationRepository.GetLocationByCode(locationCode);
            if (location == null)
            {
                throw new OcudaException("Location not found.");
            }
            else
            {
                return location;
            }
        }

        public async Task<Location> GetLocationByIdAsync(int locationId)
        {
            return await _locationRepository.FindAsync(locationId);
        }

        public async Task<Location> GetLocationByStubAsync(string locationStub)
        {
            var location = await _locationRepository.GetLocationByStub(locationStub);
            if (location == null)
            {
                throw new OcudaException("Location not found.");
            }
            else
            {
                return location;
            }
        }

        public async Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId)
        {
            return await _locationInteriorImageRepository.GetLocationInteriorImagesAsync(locationId);
        }

        public async Task<string> GetLocationLinkAsync(string placeId)
        {
            return await _googleClient.GetLocationLinkAsync(placeId);
        }

        public async Task<IDictionary<string, int>> GetLocationProductMapAsync(int productId)
        {
            var locationProductMap = await _locationProductMapRepository
                .GetByProductAsync(productId);

            return locationProductMap
                .ToDictionary(k => k.ImportLocation, v => v.LocationId);
        }

        public async Task<ICollection<Location>> GetLocationsBySegment(int segmentId)
        {
            return await _locationRepository.GetUsingSegmentAsync(segmentId);
        }

        public async Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string address)
        {
            return await _googleClient.GetLocationSummariesAsync(address);
        }

        public async Task<string> GetMapImageFilePathAsync(string filename)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            return Path.Combine(promBasePath,
                UriPaths.Images,
                UriPaths.Locations,
                UriPaths.Maps,
                filename);
        }

        public async Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(
                                            LocationFilter filter)
        {
            return await _locationRepository.GetPaginatedListAsync(filter);
        }

        public async Task<IDictionary<string, string>> GetSlugNameAsync()
        {
            return await _locationRepository.GetSlugNameAsync();
        }

        public async Task UndeleteAsync(int id)
        {
            var location = await _locationRepository.FindAsync(id);
            location.IsDeleted = false;
            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();
        }

        public async Task UpdateAltTextAsync(int locationId,
            int languageId,
            string imageFieldName,
            string altText)
        {
            var location = await _locationRepository.FindAsync(locationId);
            var segmentId = imageFieldName switch
            {
                nameof(Location.ImageAltTextSegmentId) => location.ImageAltTextSegmentId,
                nameof(Location.MapAltTextSegmentId) => location.MapAltTextSegmentId,
                _ => throw new OcudaException("Unable to determine which alt tag to update.")
            };

            if (!segmentId.HasValue)
            {
                var segment = await _segmentService.CreateAsync(new Segment
                {
                    IsActive = true,
                    Name = $"Location {location.Name} alt text {imageFieldName}",
                });

                if (imageFieldName == nameof(Location.ImageAltTextSegmentId))
                {
                    location.ImageAltTextSegmentId = segment.Id;
                }
                else if (imageFieldName == nameof(Location.MapAltTextSegmentId))
                {
                    location.MapAltTextSegmentId = segment.Id;
                }

                _locationRepository.Update(location);
                await _locationRepository.SaveAsync();

                await _segmentService.CreateSegmentTextAsync(new SegmentText
                {
                    Header = null,
                    LanguageId = languageId,
                    SegmentId = segment.Id,
                    Text = altText?.Trim()
                });
            }
            else
            {
                var segmentText = await _segmentService
                    .GetBySegmentAndLanguageAsync(segmentId.Value, languageId);

                if (segmentText == null)
                {
                    await _segmentService.CreateSegmentTextAsync(new SegmentText
                    {
                        Header = null,
                        LanguageId = languageId,
                        SegmentId = segmentId.Value,
                        Text = altText?.Trim()
                    });
                }
                else
                {
                    if (altText?.Trim().Length > 0)
                    {
                        await _segmentService.EditSegmentTextAsync(new SegmentText
                        {
                            Header = null,
                            LanguageId = languageId,
                            SegmentId = segmentId.Value,
                            Text = altText?.Trim()
                        });
                    }
                    else
                    {
                        await _segmentService.DeleteSegmentTextAsync(segmentText);
                        var segments = await _segmentService
                            .GetSegmentLanguagesByIdAsync(segmentId.Value);
                        if (segments.Count == 0)
                        {
                            if (imageFieldName == nameof(Location.ImageAltTextSegmentId))
                            {
                                location.ImageAltTextSegmentId = null;
                            }
                            else if (imageFieldName == nameof(Location.MapAltTextSegmentId))
                            {
                                location.MapAltTextSegmentId = null;
                            }
                            _locationRepository.Update(location);
                            await _locationRepository.SaveAsync();
                            await _segmentService.DeleteAsync(segmentId.Value);
                        }
                    }
                }
            }
        }

        public async Task UpdateExteriorImageAsync(IFormFile imageFile,
            string filename,
            string locationStub)
        {
            ArgumentNullException.ThrowIfNull(imageFile);

            var location = await _locationRepository.GetLocationByStub(locationStub);

            await using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            using var imagePeek = SixLabors.ImageSharp.Image.Load(fileBytes);
            if (imagePeek.Width != ExteriorImageWidth
                || imagePeek.Height != ExteriorImageHeight)
            {
                throw new OcudaException($"Please provide an image exactly {ExteriorImageWidth} pixels wide and {ExteriorImageHeight} pixels high.");
            }

            string oldImageName = location.ImagePath;

            string assumedName = !string.IsNullOrEmpty(filename)
                ? Path.HasExtension(filename)
                    ? filename : filename + Path.GetExtension(imageFile.FileName)
                : imageFile.FileName;

            location.ImagePath = await SaveImageToServerAsync(fileBytes, assumedName, false);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();

            if (location.ImagePath != oldImageName)
            {
                var oldFilePath = Path.Combine(basePath,
                    UriPaths.Images,
                    UriPaths.Locations,
                    oldImageName);
                File.Delete(oldFilePath);
                _logger.LogInformation("Exterior image {Filename} deleted", oldImageName);
            }
        }

        public async Task UpdateImageAltTextAsync(int imageId, int languageId, string altText)
        {
            var altTextObject = await _imageAltTextRepository
                .GetImageAltTextAsync(imageId, languageId);
            altTextObject.AltText = altText;
            _imageAltTextRepository.Update(altTextObject);
            await _imageAltTextRepository.SaveAsync();
        }

        public async Task UpdateInteriorImageAsync(LocationInteriorImage newInteriorImage,
            string filename,
            IFormFile imageFile)
        {
            ArgumentNullException.ThrowIfNull(newInteriorImage);
            ArgumentNullException.ThrowIfNull(imageFile);

            var interiorImage = await GetInteriorImageByIdAsync(newInteriorImage.Id);

            await using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            using var imagePeek = SixLabors.ImageSharp.Image.Load(fileBytes);
            if (imagePeek.Width != InteriorImageWidth
                || imagePeek.Height != InteriorImageHeight)
            {
                throw new OcudaException($"Please provide an image exactly {InteriorImageWidth} pixels wide and {InteriorImageHeight} pixels high.");
            }

            string oldImageName = interiorImage.ImagePath;

            string assumedName = !string.IsNullOrEmpty(filename)
                ? Path.HasExtension(filename)
                    ? filename : filename + Path.GetExtension(imageFile.FileName)
                : imageFile.FileName;

            interiorImage.ImagePath = await SaveImageToServerAsync(fileBytes,
                assumedName,
                false,
                UriPaths.Images);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            _locationInteriorImageRepository.Update(interiorImage);
            await _locationInteriorImageRepository.SaveAsync();

            if (interiorImage.ImagePath != oldImageName)
            {
                var oldFilePath = Path.Combine(basePath,
                    UriPaths.Images,
                    UriPaths.Locations,
                    UriPaths.Interior,
                    oldImageName);
                File.Delete(oldFilePath);
                _logger.LogInformation("Old interior image {Filename} deleted", oldImageName);
            }
        }

        public async Task UpdateInteriorImageSortAsync(string slug, int interiorImageId, int increment)
        {
            var location = await GetLocationByStubAsync(slug)
                ?? throw new OcudaException($"Can't find location with slug {slug}");

            var interiorImages = await _locationInteriorImageRepository
                .GetLocationInteriorImagesAsync(location.Id);

            var item = interiorImages.SingleOrDefault(_ => _.Id == interiorImageId)
                ?? throw new OcudaException($"Unable to find interior image id {interiorImageId}");

            var newSort = item.SortOrder + increment;

            var itemsToChange = interiorImages.Where(_ => _.SortOrder == newSort
                && _.Id != interiorImageId);

            item.SortOrder = newSort;

            var itemsToUpdate = new List<LocationInteriorImage> { item };

            foreach (var itemToChange in itemsToChange)
            {
                itemToChange.SortOrder += -1 * increment;
                itemsToUpdate.Add(itemToChange);
            }

            _locationInteriorImageRepository.UpdateRange(itemsToUpdate);
            await _locationInteriorImageRepository.SaveAsync();

            await FixInteriorImageSortOrder(location.Id);
        }

        public async Task UpdateLocationMappingAsync(int locationMapId, string importLocation, int locationId)
        {
            var existing = await _locationProductMapRepository.FindAsync(locationMapId)
                ?? throw new OcudaException("Unable to find that location map.");

            existing.ImportLocation = importLocation;
            existing.LocationId = locationId;

            try
            {
                _locationProductMapRepository.Update(existing);
                await _locationProductMapRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new OcudaException(ex.Message, ex);
            }
        }

        public async Task UpdateMapImageAsync(byte[] fileBytes,
            string filename,
            string locationStub)
        {
            ArgumentNullException.ThrowIfNull(fileBytes);

            var location = await _locationRepository.GetLocationByStub(locationStub);

            string oldImageName = location.MapImagePath;

            string assumedName = filename;

            location.MapImagePath = await SaveImageToServerAsync(fileBytes,
                assumedName,
                true,
                UriPaths.Maps);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();

            if (location.MapImagePath != oldImageName)
            {
                var oldFilePath = Path.Combine(basePath,
                    UriPaths.Images,
                    UriPaths.Locations,
                    UriPaths.Maps,
                    oldImageName);
                File.Delete(oldFilePath);
                _logger.LogInformation("Map image {Filename} deleted", oldImageName);
            }
        }

        public async Task UploadAddInteriorImageAsync(int locationId,
            string filename,
            IFormFile imageFile,
            IDictionary<int, string> altTexts)
        {
            ArgumentNullException.ThrowIfNull(altTexts);
            ArgumentNullException.ThrowIfNull(imageFile);

            var locationInteriorImage = new LocationInteriorImage
            {
                LocationId = locationId,
                SortOrder = await _locationInteriorImageRepository.GetNextSortOrderAsync(locationId)
            };

            await using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            using var imagePeek = SixLabors.ImageSharp.Image.Load(fileBytes);
            if (imagePeek.Width != InteriorImageWidth
                || imagePeek.Height != InteriorImageHeight)
            {
                throw new OcudaException($"Please provide an image exactly {InteriorImageWidth} pixels wide and {InteriorImageHeight} pixels high.");
            }

            string assumedName = !string.IsNullOrEmpty(filename)
                ? Path.HasExtension(filename)
                    ? filename : filename + Path.GetExtension(imageFile.FileName)
                : imageFile.FileName;

            locationInteriorImage.ImagePath = await SaveImageToServerAsync(fileBytes,
                assumedName,
                false,
                UriPaths.Interior);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            await AddInteriorImageAsync(locationInteriorImage);

            var altTextToInsert = new List<LocationInteriorImageAltText>();
            foreach (var altText in altTexts)
            {
                altTextToInsert.Add(new LocationInteriorImageAltText
                {
                    AltText = altText.Value,
                    LanguageId = altText.Key,
                    LocationInteriorImageId = locationInteriorImage.Id,
                });
            }

            if (altTextToInsert?.Count > 0)
            {
                await AddAltTextRangeAsync(altTextToInsert);
            }
        }

        public async Task UploadLocationMapAsync(byte[] imageBytes, string fileName)
        {
            ArgumentNullException.ThrowIfNull(imageBytes);
            ArgumentNullException.ThrowIfNull(fileName);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var filePath = Path.Combine(basePath,
                UriPaths.Images,
                UriPaths.Locations,
                UriPaths.Maps);

            try
            {
                if (!Directory.Exists(filePath))
                {
                    _logger.LogInformation("Creating location map image directory: {Path}",
                        filePath);
                    Directory.CreateDirectory(filePath);
                }

                var fileWritePath = Path.Combine(filePath, fileName);

                await File.WriteAllBytesAsync(fileWritePath, imageBytes);

                var assetBase = Path.DirectorySeparatorChar + UriPaths.Assets;

                var assetPath = Path.Combine(assetBase,
                    UriPaths.Images,
                    UriPaths.Locations,
                    UriPaths.Maps,
                    fileName);

                var locationCode = Path.GetFileNameWithoutExtension(fileName);

                var location = await _locationRepository.GetLocationByCode(locationCode);

                var oldFileName = Path.GetFileName(location.MapImagePath);

                location.MapImagePath = assetPath;

                _locationRepository.Update(location);
                await _locationRepository.SaveAsync();

                if (fileName != oldFileName)
                {
                    var oldFilePath = Path.Combine(filePath, oldFileName);
                    File.Delete(oldFilePath);
                    _logger.LogInformation("Deleted map image {Path}",
                        oldFilePath);
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogError("Error uploading map image: {ErrorMessage}",
                    oex.Message);
                throw new OcudaException($"Error uploading map image: {oex.Message}");
            }
        }

        private static string GetFormattedDayGroupings(List<DayOfWeek> days)
        {
            var dayFormatter = new DateTimeFormatInfo();
            if (days.Count == 1)
            {
                return dayFormatter.GetAbbreviatedDayName(days[0]);
            }
            else
            {
                var firstDay = days[0];
                var lastDay = days[^1];

                if (days.Count == lastDay - firstDay + 1)
                {
                    return $"{dayFormatter.GetAbbreviatedDayName(firstDay)}{ndash}{dayFormatter.GetAbbreviatedDayName(lastDay)}";
                }
                else if (days.Count == 2)
                {
                    return $"{dayFormatter.GetAbbreviatedDayName(firstDay)} & {dayFormatter.GetAbbreviatedDayName(lastDay)}";
                }
                else
                {
                    return string.Join(", ", days.Select(_ => dayFormatter.GetAbbreviatedDayName(_)));
                }
            }
        }

        private async Task FixInteriorImageSortOrder(int locationId)
        {
            await _locationInteriorImageRepository.FixInteriorImageSortOrder(locationId);
        }

        private async Task<string> SaveImageToServerAsync(byte[] imageBytes,
            string requestedFilename,
            bool overwrite)
        {
            return await SaveImageToServerAsync(imageBytes, requestedFilename, overwrite, string.Empty);
        }

        private async Task<string> SaveImageToServerAsync(byte[] imageBytes,
            string requestedFilename,
            bool overwrite,
            string subDirectory)
        {
            ArgumentNullException.ThrowIfNull(imageBytes);
            ArgumentNullException.ThrowIfNull(requestedFilename);

            string tempFilePath = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName())
                + Path.GetExtension(requestedFilename));

            File.WriteAllBytes(tempFilePath, imageBytes);

            try
            {
                try
                {
                    var optimized = await _imageService.OptimizeAsync(tempFilePath);
                    imageBytes = optimized.File;
                }
                catch (ParameterException pex)
                {
                    throw new OcudaException($"Error optimizing file: {pex.Message}", pex);
                }
                catch (OcudaConfigurationException)
                { }

                string basePath = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

                var filePath = Path.Combine(basePath,
                    UriPaths.Images,
                    UriPaths.Locations,
                    subDirectory);

                if (!Directory.Exists(filePath))
                {
                    _logger.LogInformation("Creating image subdirectory: {Path}",
                        filePath);
                    Directory.CreateDirectory(filePath);
                }

                var format = SixLabors.ImageSharp.Image.DetectFormat(imageBytes);

                var filename = Path.GetFileNameWithoutExtension(requestedFilename)
                    + "."
                    + format.FileExtensions.First();

                var fileWritePath = Path.Combine(filePath, filename);

                int renameCounter = 1;
                while (!overwrite && File.Exists(fileWritePath))
                {
                    filename = string.Format(CultureInfo.InvariantCulture,
                        "{0}-{1}.{2}",
                        Path.GetFileNameWithoutExtension(requestedFilename),
                        renameCounter++,
                        format.FileExtensions.First());
                    fileWritePath = Path.Combine(filePath, filename);
                }

                await File.WriteAllBytesAsync(fileWritePath, imageBytes);

                var assetBase = Path.DirectorySeparatorChar + UriPaths.Assets;

                return filename;
            }
            catch (OcudaException oex)
            {
                _logger.LogError("Error saving image to server: {ErrorMessage}",
                    oex.Message);
                throw;
            }
        }

        private async Task ValidateAsync(Location location)
        {
            if (await _locationRepository.IsDuplicateNameAsync(location))
            {
                throw new OcudaException($"Location Name '{location.Name}' already exists.");
            }
            if (await _locationRepository.IsDuplicateStubAsync(location))
            {
                throw new OcudaException($"Location Stub '{location.Stub}' already exists.");
            }
        }
    }
}