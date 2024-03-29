using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
        private readonly ISiteSettingService _siteSettingService;
        private readonly string AssetBasePath = "assets";
        private readonly string ImageFilePath = "images";
        private readonly string LocationFilePath = "locations";
        private readonly string MapFilePath = "maps";

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
            _siteSettingService = siteSettingService;
        }

        public async Task AddAltTextRangeAsync(List<ImageAltText> imageAltTexts)
        {
            ArgumentNullException.ThrowIfNull(imageAltTexts);

            foreach (var altText in imageAltTexts)
            {
                altText.AltText = altText.AltText.Trim();
            }

            await _imageAltTextRepository.AddRangeAsync(imageAltTexts);
            await _imageAltTextRepository.SaveAsync();
        }

        public async Task AddImageAltTextAsync(ImageAltText imageAltText)
        {
            await _imageAltTextRepository.AddAsync(imageAltText);
            await _locationInteriorImageRepository.SaveAsync();
        }

        public async Task AddInteriorImageAsync(LocationInteriorImage locationInteriorImage)
        {
            await _locationInteriorImageRepository.AddAsync(locationInteriorImage);
            await _locationInteriorImageRepository.SaveAsync();
        }

        public async Task<Location> AddLocationAsync(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

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

        public async Task<string> AssetPathToFullPath(string imagePath)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Ocuda.Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var filePath = ImageFilePath + HttpUtility.UrlDecode(imagePath).Split(ImageFilePath).Last();

            return Path.Combine(promBasePath, filePath);
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
            var imageAltTexts = await _imageAltTextRepository.GetAllLanguageImageAltTextsAsync(imageId);

            var imagePath = await AssetPathToFullPath(image.ImagePath);

            _locationInteriorImageRepository.Remove(image);
            _imageAltTextRepository.RemoveRange(imageAltTexts);

            await _imageAltTextRepository.SaveAsync();

            File.Delete(imagePath);
        }

        public async Task DeleteMappingAsync(int locationMapId)
        {
            var mapping = await _locationProductMapRepository.FindAsync(locationMapId);
            _locationProductMapRepository.Remove(mapping);
            await _locationProductMapRepository.SaveAsync();
        }

        public async Task<Location> EditAlwaysOpenAsync(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var currentLocation = await _locationRepository.FindAsync(location.Id);
            currentLocation.IsAlwaysOpen = location.IsAlwaysOpen;

            await ValidateAsync(currentLocation);

            _locationRepository.Update(currentLocation);
            await _locationRepository.SaveAsync();
            return currentLocation;
        }

        public async Task<Location> EditAsync(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            await ValidateAsync(location);

            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();
            return location;
        }

        public async Task<List<ImageAltText>> GetAllLanguageImageAltTextsAsync(int imageId)
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

        public async Task<ImageAltText> GetImageAltTextAsync(int imageId, int languageId)
        {
            return await _imageAltTextRepository.GetImageAltTextAsync(imageId, languageId);
        }

        public async Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId)
        {
            return await _locationInteriorImageRepository.GetInteriorImageByIdAsync(imageId);
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

        public async Task<DataWithCount<ICollection<Location>>> GetPaginatedListAsync(
                                            LocationFilter filter)
        {
            return await _locationRepository.GetPaginatedListAsync(filter);
        }

        public async Task<string> SaveImageToServerAsync(byte[] imageBytes, string fileName)
        {
            return await SaveImageToServerAsync(imageBytes, fileName, string.Empty);
        }

        public async Task<string> SaveImageToServerAsync(byte[] imageBytes,
            string fileName,
            string subDirectory)
        {
            ArgumentNullException.ThrowIfNull(imageBytes);
            ArgumentNullException.ThrowIfNull(fileName);

            string tempFilePath = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName())
                + Path.GetExtension(fileName));

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
                    ImageFilePath,
                    LocationFilePath,
                    subDirectory);

                if (!Directory.Exists(filePath))
                {
                    _logger.LogInformation("Creating image card directory: {Path}",
                        filePath);
                    Directory.CreateDirectory(filePath);
                }

                var fileWritePath = Path.Combine(filePath, fileName);

                await File.WriteAllBytesAsync(fileWritePath, imageBytes);

                var assetBase = Path.DirectorySeparatorChar + AssetBasePath;

                return Path.Combine(assetBase,
                    ImageFilePath,
                    LocationFilePath,
                    subDirectory,
                    fileName);
            }
            catch (OcudaException oex)
            {
                _logger.LogError("Error saving image to server: {ErrorMessage}",
                    oex.Message);
                throw;
            }
        }

        public async Task UndeleteAsync(int id)
        {
            var location = await _locationRepository.FindAsync(id);
            location.IsDeleted = false;
            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();
        }

        public async Task UpdateExteriorImage(IFormFile imageFile, string locationStub)
        {
            ArgumentNullException.ThrowIfNull(imageFile);

            var location = await _locationRepository.GetLocationByStub(locationStub);

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var imageRelativePath = await SaveImageToServerAsync(
                fileBytes,
                imageFile.FileName);

            var imageName = imageFile.FileName;
            var oldImageName = Path.GetFileName(location.ImagePath);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var imageDirectoryPath = Path.Combine(basePath,
                ImageFilePath,
                LocationFilePath);

            location.ImagePath = imageRelativePath;

            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();

            if (imageName != oldImageName)
            {
                var oldFilePath = Path.Combine(imageDirectoryPath, oldImageName);
                File.Delete(oldFilePath);
            }
        }

        public async Task UpdateInteriorImageAsync(LocationInteriorImage newInteriorImage, IFormFile imageFile)
        {
            ArgumentNullException.ThrowIfNull(newInteriorImage);

            var interiorImage = await GetInteriorImageByIdAsync(newInteriorImage.Id);

            interiorImage.SortOrder = newInteriorImage.SortOrder;

            if (imageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var imageRelativePath = await SaveImageToServerAsync(
                    fileBytes,
                    imageFile.FileName);

                var imageName = imageFile.FileName;
                var oldImageName = Path.GetFileName(interiorImage.ImagePath);

                string basePath = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

                var imageDirectoryPath = Path.Combine(basePath,
                    ImageFilePath,
                    LocationFilePath);

                interiorImage.ImagePath = imageRelativePath;

                if (imageName != oldImageName)
                {
                    var oldFilePath = Path.Combine(imageDirectoryPath, oldImageName);
                    File.Delete(oldFilePath);
                }
            }

            var imageAltTexts = await GetAllLanguageImageAltTextsAsync(newInteriorImage.Id);

            foreach (var altText in imageAltTexts)
            {
                altText.AltText = newInteriorImage.AllAltTexts
                    .Where(_ => _.LanguageId == altText.LanguageId)
                    .Single()
                    .AltText;
            }

            _locationInteriorImageRepository.Update(interiorImage);
            _imageAltTextRepository.UpdateRange(imageAltTexts);
            await _imageAltTextRepository.SaveAsync();
        }

        public async Task UpdateLocationMapPathAsync(string locationCode, string mapImagePath)
        {
            var location = await _locationRepository.GetLocationByCode(locationCode);

            var fileName = Path.GetFileName(mapImagePath);
            var oldFileName = Path.GetFileName(location.MapImagePath);

            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var filePath = Path.Combine(basePath,
                ImageFilePath,
                LocationFilePath,
                MapFilePath);

            location.MapImagePath = mapImagePath;

            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();

            if (fileName != oldFileName)
            {
                var oldFilePath = Path.Combine(filePath, oldFileName);
                File.Delete(oldFilePath);
            }
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
                var lastDay = days.Last();

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