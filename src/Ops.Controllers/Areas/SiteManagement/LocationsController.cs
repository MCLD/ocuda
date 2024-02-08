using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ImageOptimApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly string _apiKey;
        private readonly string ImageFilePath = "images";
        private readonly string LocationFilePath = "locations";
        private readonly string MapFilePath = "maps";
        private readonly IFeatureService _featureService;
        private readonly IGroupService _groupService;
        private readonly ILanguageService _languageService;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationGroupService _locationGroupService;
        private readonly ILocationHoursService _locationHoursService;
        private readonly ILocationService _locationService;
        private readonly ISegmentService _segmentService;
        private readonly ISocialCardService _socialCardService;
        private readonly IVolunteerFormService _volunteerFormService;
        private readonly IImageService _imageService;

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            IConfiguration config,
            IFeatureService featureService,
            IGroupService groupService,
            ILanguageService languageService,
            ILocationFeatureService locationFeatureService,
            ILocationGroupService locationGroupService,
            ILocationHoursService locationHoursService,
            ILocationService locationService,
            ISegmentService segmentService,
            ISocialCardService socialCardService,
            IVolunteerFormService volunteerFormService,
            IImageService imageService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(featureService);
            ArgumentNullException.ThrowIfNull(groupService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(locationFeatureService);
            ArgumentNullException.ThrowIfNull(locationGroupService);
            ArgumentNullException.ThrowIfNull(locationHoursService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(socialCardService);
            ArgumentNullException.ThrowIfNull(volunteerFormService);
            ArgumentNullException.ThrowIfNull(imageService);

            _featureService = featureService;
            _groupService = groupService;
            _languageService = languageService;
            _locationFeatureService = locationFeatureService;
            _locationGroupService = locationGroupService;
            _locationHoursService = locationHoursService;
            _locationService = locationService;
            _segmentService = segmentService;
            _socialCardService = socialCardService;
            _volunteerFormService = volunteerFormService;
            _imageService = imageService;

            _apiKey = config[Configuration.OcudaGoogleAPI];
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Locations"; } }

        [HttpGet("[action]")]
        [SaveModelState]
        public IActionResult AddLocation()
        {
            var location = new Location
            {
                IsNewLocation = true
            };
            var viewModel = new LocationViewModel
            {
                Location = location,
                Action = nameof(LocationsController.CreateLocation)
            };

            return View("LocationDetails", viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState(Key = nameof(Hours))]
        public async Task<IActionResult> AddOverride(LocationHoursViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (model.AddOverride.Open)
            {
                if (!model.AddOverride.OpenTime.HasValue || !model.AddOverride.CloseTime.HasValue)
                {
                    if (!model.AddOverride.OpenTime.HasValue)
                    {
                        ModelState.AddModelError("AddOverride.OpenTime",
                            "Please select an Open Time.");
                    }
                    if (!model.AddOverride.CloseTime.HasValue)
                    {
                        ModelState.AddModelError("AddOverride.CloseTime",
                            "Please select a Close Time.");
                    }
                }
                else if (model.AddOverride.OpenTime > model.AddOverride.CloseTime)
                {
                    ModelState.AddModelError("AddOverride.OpenTime",
                        "Open Time must be before the Close Time.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hoursOverride = await _locationHoursService
                        .AddLocationHoursOverrideAsync(model.AddOverride);
                    ShowAlertSuccess($"Override '{hoursOverride.Reason}' added!");
                }
                catch (OcudaException gex)
                {
                    ModelState.AddModelError("AddOverride.Date", gex.Message);
                }
            }

            return RedirectToAction(nameof(Hours), new { locationStub = model.LocationStub });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]/{locationStub}")]
        public async Task<IActionResult> AddSegment(string locationStub,
            string whichSegment,
            string segmentText)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                ShowAlertDanger("Invalid add segment request: no location specified.");
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(whichSegment))
            {
                ShowAlertDanger("Invalid add segment request: no segment specified.");
                return RedirectToAction(nameof(Location), new { locationStub });
            }

            var validSegments = new Dictionary<string, string>{
                { "Description", "Description"},
                { "HoursOverride", "Hours Override"},
                { "PreFeature", "Pre-feature"},
                { "PostFeature", "Post-feature"},
            };

            if (!validSegments.TryGetValue(whichSegment, out string value))
            {
                ShowAlertDanger($"Invalid add segment request: unknown segment: {whichSegment}");
                return RedirectToAction(nameof(Location), new { locationStub });
            }

            var location = await _locationService.GetLocationByStubAsync(locationStub);

            if (location == null)
            {
                ShowAlertDanger($"Location not found for stub {locationStub}.");
                return RedirectToAction(nameof(Index));
            }

            var languages = await _languageService.GetActiveAsync();

            var defaultLanguage = languages.SingleOrDefault(_ => _.IsActive && _.IsDefault)
                ?? languages.FirstOrDefault(_ => _.IsActive);

            if (defaultLanguage == null)
            {
                ShowAlertDanger("No default language configured.");
                return RedirectToAction(nameof(Location), new { locationStub });
            }

            var segment = await _segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"{location.Name} - {value}",
            });

            await _segmentService.CreateSegmentTextAsync(new SegmentText
            {
                SegmentId = segment.Id,
                LanguageId = defaultLanguage.Id,
                Text = segmentText
            });

            // get location, create segment
            switch (whichSegment.Trim().ToUpperInvariant())
            {
                case "HOURSOVERRIDE":
                    location.HoursSegmentId = segment.Id;
                    break;

                case "PREFEATURE":
                    location.PreFeatureSegmentId = segment.Id;
                    break;

                case "POSTFEATURE":
                    location.PostFeatureSegmentId = segment.Id;
                    break;

                default:
                    location.DescriptionSegmentId = segment.Id;
                    break;
            }

            await _locationService.EditAsync(location);

            return RedirectToAction(nameof(Location), new { locationStub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateLocation(Location location)
        {
            if (location != null && ModelState.IsValid)
            {
                if (location?.Phone?.Length == 10)
                {
                    location.Phone =
                        $"+1 {Convert.ToInt64(location.Phone, CultureInfo.InvariantCulture):###-###-####}";
                }

                try
                {
                    await _locationService.AddLocationAsync(location);
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        var locationHours = new LocationHours
                        {
                            DayOfWeek = day,
                            Open = false,
                            Location = location,
                            LocationId = location.Id
                        };
                        await _locationHoursService.AddLocationHoursAsync(locationHours);
                    }
                    ShowAlertSuccess($"Added Location: {location.Name}");
                    location.IsNewLocation = true;
                    return RedirectToAction(nameof(LocationsController.Location),
                        new { locationStub = location.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Create Location: {ex.Message}");
                    location.IsNewLocation = true;

                    return View("LocationDetails", new LocationViewModel
                    {
                        Location = location,
                        Action = nameof(LocationsController.CreateLocation)
                    });
                }
            }
            return RedirectToAction(nameof(LocationsController.AddLocation));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateLocationFeature(int locationId, int itemId)
        {
            var location = await _locationService.GetLocationByIdAsync(locationId);
            var feature = await _featureService.GetFeatureByIdAsync(itemId);
            try
            {
                var locationFeature = new LocationFeature
                {
                    FeatureId = itemId,
                    LocationId = locationId
                };
                await _locationFeatureService.AddLocationFeatureAsync(locationFeature);
                ShowAlertSuccess($"Added feature '{feature.Name}' to location '{location.Name}'");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Add feature to Location: {ex.Message}");
                _logger.LogError(ex,
                    "Failed to Add {FeatureName} to {LocationName}: {Message}",
                    feature.Name,
                    location.Name,
                    ex.Message);
            }

            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateLocationGroup(int locationId, int itemId)
        {
            var location = await _locationService.GetLocationByIdAsync(locationId);
            var group = await _groupService.GetGroupByIdAsync(itemId);
            try
            {
                var locationGroup = new LocationGroup
                {
                    GroupId = itemId,
                    LocationId = locationId
                };
                if (!string.IsNullOrEmpty(group.SubscriptionUrl))
                {
                    locationGroup.HasSubscription = true;
                }
                await _locationGroupService.AddLocationGroupAsync(locationGroup);
                ShowAlertSuccess($"Added group '{group.GroupType}' to location '{location.Name}'");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Add group to Location: {ex.Message}");
                _logger.LogError(ex,
                    "Failed to add {Group} to {Name}: {Message}",
                    group.GroupType,
                    location.Name,
                    ex.Message);
            }

            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Deleted(int page)
        {
            page = page == 0 ? 1 : page;
            return await LocationListAsync(new LocationFilter(page == 0 ? 1 : page)
            {
                IsDeleted = true
            });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteLocation(Location location)
        {
            ArgumentNullException.ThrowIfNull(location);
            try
            {
                await _locationService.DeleteAsync(location.Id);
                ShowAlertSuccess($"Deleted Location: {location.Name}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Delete Location {location.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(LocationsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteLocationFeature(int itemId, int locationId)
        {
            var feature = await _featureService.GetFeatureByIdAsync(itemId);
            var location = await _locationService.GetLocationByIdAsync(locationId);
            try
            {
                await _locationFeatureService.DeleteAsync(itemId, locationId);
                ShowAlertSuccess($"Deleted Feature '{feature.Name}' from '{location.Name}'");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to delete feature '{feature.Name}' from '{location.Name}': {ex.Message}");
                _logger.LogError(ex,
                    "Problem deleting feature {FeatureName} from {LocationName}: {Message}",
                    feature.Name,
                    location.Name,
                    ex.Message);
            }
            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteLocationGroup(int itemId, int locationId)
        {
            var group = await _groupService.GetGroupByIdAsync(itemId);
            var location = await _locationService.GetLocationByIdAsync(locationId);

            try
            {
                await _locationGroupService.DeleteAsync(itemId, locationId);
                ShowAlertSuccess($"Deleted Group '{group.GroupType}' from '{location.Name}'");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to delete group '{group.GroupType}' from '{location.Name}': {ex.Message}");
                _logger.LogError(ex,
                    "Problem deleting group {Group} from {LocationName}: {Message}",
                    group.GroupType,
                    location.Name,
                    ex.Message);
            }

            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteOverride(LocationHoursViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                await _locationHoursService.DeleteLocationsHoursOverrideAsync(
                    model.DeleteOverride.Id);
                ShowAlertSuccess($"Deleted override: {model.DeleteOverride.Reason}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex,
                    "Error deleting override id {Id}: {Message}",
                    model.DeleteOverride.Id,
                    ex.Message);
                ShowAlertDanger("Unable to delete override: ", ex.Message);
            }

            return RedirectToAction(nameof(Hours), new { locationStub = model.LocationStub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditLocation(Location location)
        {
            ArgumentNullException.ThrowIfNull(location);

            if (ModelState.IsValid)
            {
                try
                {
                    var currentLocation = await _locationService.GetLocationByIdAsync(location.Id);
                    currentLocation.Address = location.Address?.Trim();
                    currentLocation.City = location.City?.Trim();
                    currentLocation.Country = location.Country?.Trim();
                    currentLocation.Code = location.Code?.Trim();
                    currentLocation.EventLink = location.EventLink?.Trim();
                    currentLocation.Facebook = location.Facebook?.Trim();
                    currentLocation.GeoLocation = location.GeoLocation?.Trim();
                    currentLocation.MapLink = location.MapLink?.Trim();
                    currentLocation.Name = location.Name?.Trim();
                    currentLocation.Phone = location.Phone?.Trim();
                    currentLocation.State = location.State?.Trim();
                    currentLocation.Stub = location.Stub?.Trim();
                    currentLocation.SubscriptionLink = location.SubscriptionLink?.Trim();
                    currentLocation.Zip = location.Zip?.Trim();
                    currentLocation.IsNewLocation = false;

                    var updatedLocation = await _locationService.EditAsync(currentLocation);

                    ShowAlertSuccess($"Updated location: {updatedLocation.Name}");

                    return RedirectToAction(nameof(LocationsController.Location),
                        new { locationStub = updatedLocation.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to update location {location.Name}: {ex.Message}");
                    _logger.LogError(ex,
                        "Problem updating location {LocationName}: {Message}",
                        location.Name,
                        ex.Message);
                    location.IsNewLocation = true;
                    var viewModel = new LocationViewModel
                    {
                        Location = location,
                        Action = nameof(LocationsController.EditLocation)
                    };

                    return View("LocationDetails", viewModel);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditLocationFeature(LocationFeature locationFeature)
        {
            ArgumentNullException.ThrowIfNull(locationFeature);

            var location = await _locationService.GetLocationByIdAsync(locationFeature.LocationId);
            if (ModelState.IsValid)
            {
                try
                {
                    await _locationFeatureService.EditAsync(locationFeature);
                    var feature = await _featureService
                        .GetFeatureByIdAsync(locationFeature.FeatureId);
                    ShowAlertSuccess($"Updated {location.Name}'s Feature: {feature.Name}");
                }
                catch (OcudaException ex)
                {
                    var feature = await _featureService
                        .GetFeatureByIdAsync(locationFeature.FeatureId);
                    ShowAlertDanger($"Failed to Update {location.Name}'s Feature: {feature.Name}");
                    _logger.LogError(ex,
                        "Unable to edit feature {FeatureName} for location {LocationName}: {Message}",
                        feature.Name,
                        location.Name,
                        ex.Message);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditLocationGroup(LocationViewModel locationGroupInfo)
        {
            ArgumentNullException.ThrowIfNull(locationGroupInfo);

            var location = await _locationService
                .GetLocationByIdAsync(locationGroupInfo.LocationGroup.LocationId);

            if (ModelState.IsValid)
            {
                try
                {
                    if (locationGroupInfo.IsLocationsGroup)
                    {
                        location.DisplayGroupId = locationGroupInfo.LocationGroup.GroupId;
                    }
                    else
                    {
                        location.DisplayGroupId = null;
                    }
                    await _locationService.EditAsync(location);
                    var group = await _groupService
                        .GetGroupByIdAsync(locationGroupInfo.LocationGroup.GroupId);
                    ShowAlertSuccess($"Updated {location.Name}'s Group: {group.GroupType}");
                }
                catch (OcudaException ex)
                {
                    var group = await _groupService
                        .GetGroupByIdAsync(locationGroupInfo.LocationGroup.GroupId);
                    ShowAlertDanger($"Problem Updating {location.Name}'s Group: {group.GroupType}");
                    _logger.LogError(ex,
                        "Problem updating group {Group} for location {LocationName}: {Message}",
                        group.GroupType,
                        location.Name,
                        ex.Message);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location),
                new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState(Key = nameof(Hours))]
        public async Task<IActionResult> EditOverride(LocationHoursViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (model.EditOverride.Open)
            {
                if (!model.EditOverride.OpenTime.HasValue
                    || !model.EditOverride.CloseTime.HasValue)
                {
                    if (!model.EditOverride.OpenTime.HasValue)
                    {
                        ModelState.AddModelError("EditOverride.OpenTime",
                            "Please select an Open Time.");
                    }
                    if (!model.EditOverride.CloseTime.HasValue)
                    {
                        ModelState.AddModelError("EditOverride.CloseTime",
                            "Please select a Close Time.");
                    }
                }
                else if (model.EditOverride.OpenTime > model.EditOverride.CloseTime)
                {
                    ModelState.AddModelError("EditOverride.OpenTime",
                        "Open Time must be before the Close Time.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hoursOverride = await _locationHoursService
                        .EditLocationHoursOverrideAsync(model.EditOverride);
                    ShowAlertSuccess($"Override '{hoursOverride.Reason}' updated!");
                }
                catch (OcudaException gex)
                {
                    ModelState.AddModelError("EditOverride.Date", gex.Message);
                }
            }

            return RedirectToAction(nameof(Hours), new { locationStub = model.LocationStub });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetCoordinates(string address)
        {
            string message;
            if (string.IsNullOrEmpty(_apiKey))
            {
                message = $"Please configure a Google API key with maps access in setting: {Configuration.OcudaGoogleAPI}";
            }
            else if (string.IsNullOrEmpty(address))
            {
                message = "You must supply an address to geocode.";
            }
            else
            {
                var (latitude, longitude) = await _locationService.GetCoordinatesAsync(address);

                if (latitude.HasValue && longitude.HasValue)
                {
                    return Json(new
                    {
                        success = true,
                        latitude = latitude.Value,
                        longitude = longitude.Value
                    });
                }
                else
                {
                    message = $"Unable to geocode address: {address}";
                }
            }

            return Json(new
            {
                success = false,
                message
            });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetItemInfo(int itemId, string objectType,
            string locationStub)
        {
            var location = await _locationService.GetLocationByStubAsync(locationStub);
            var viewModel = new LocationViewModel
            {
                Location = location
            };

            if (objectType == "Group")
            {
                viewModel.LocationGroup = await _locationGroupService
                    .GetByIdsAsync(itemId, location.Id);
                viewModel.Group = await _groupService
                    .GetGroupByIdAsync(viewModel.LocationGroup.GroupId);
                viewModel.IsLocationsGroup = location.DisplayGroupId == viewModel.Group.Id;
                return PartialView("_EditGroupsPartial", viewModel);
            }
            else
            {
                viewModel.LocationFeature = await _locationFeatureService
                    .GetByIdsAsync(itemId, location.Id);
                viewModel.Features = await _featureService.GetAllFeaturesAsync();
                return PartialView("_EditFeaturesPartial", viewModel);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetItemsList(string itemIds,
            string objectType,
            int page)
        {
            if (page == 0)
            {
                page = 1;
            }

            if (objectType == "Group")
            {
                var filter = new GroupFilter(page, 10);

                if (!string.IsNullOrWhiteSpace(itemIds))
                {
                    filter.GroupIds = itemIds.Split(',')
                        .Where(_ => !string.IsNullOrWhiteSpace(_))
                        .Select(int.Parse)
                        .ToList();
                }
                var items = await _groupService.PageItemsAsync(filter);
                var paginateModel = new PaginateModel
                {
                    ItemCount = items.Count,
                    CurrentPage = page,
                    ItemsPerPage = filter.Take.Value
                };

                return PartialView("_AddGroupsPartial", new GroupListViewModel
                {
                    Groups = items.Data,
                    PaginateModel = paginateModel
                });
            }
            else
            {
                var filter = new FeatureFilter(page, 10);

                if (!string.IsNullOrWhiteSpace(itemIds))
                {
                    filter.FeatureIds = itemIds.Split(',')
                        .Where(_ => !string.IsNullOrWhiteSpace(_))
                        .Select(int.Parse)
                        .ToList();
                }
                var items = await _featureService.PageItemsAsync(filter);
                var paginateModel = new PaginateModel
                {
                    ItemCount = items.Count,
                    CurrentPage = page,
                    ItemsPerPage = filter.Take.Value
                };

                return PartialView("_AddFeaturesPartial", new FeatureListViewModel
                {
                    Features = items.Data,
                    PaginateModel = paginateModel
                });
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetLocationLink(string placeId)
        {
            string message;
            if (string.IsNullOrEmpty(_apiKey))
            {
                message = $"Please configure a Google API key with maps access in setting: {Configuration.OcudaGoogleAPI}";
            }
            else if (string.IsNullOrEmpty(placeId))
            {
                message = "Place id is required to get details.";
            }
            else
            {
                var link = await _locationService.GetLocationLinkAsync(placeId);

                if (!string.IsNullOrEmpty(link))
                {
                    return Json(new
                    {
                        success = true,
                        link
                    });
                }
                else
                {
                    message = $"Unable to find link for place id: {placeId}";
                }
            }

            return Json(new
            {
                success = false,
                message
            });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetLocations(string name, string address)
        {
            string message;
            if (string.IsNullOrEmpty(_apiKey))
            {
                message = $"Please configure a Google API key with maps access in setting: {Configuration.OcudaGoogleAPI}";
            }
            else if (string.IsNullOrEmpty(address))
            {
                message = "You must supply an address to search locations.";
            }
            else
            {
                if (!string.IsNullOrEmpty(name))
                {
                    address = $"{name} {address.Trim(',').Replace(',', ' ')}";
                }

                try
                {
                    var locations = await _locationService.GetLocationSummariesAsync(address);

                    if (locations != null)
                    {
                        return Json(new
                        {
                            success = true,
                            locations
                        });
                    }
                    else
                    {
                        message = $"Unable to find locations for: {address}";
                    }
                }
                catch (OcudaException oex)
                {
                    message = oex.Message;
                }
            }

            return Json(new
            {
                success = false,
                message
            });
        }

        [HttpGet]
        [Route("{locationStub}/[action]")]
        [RestoreModelState(Key = nameof(Hours))]
        public async Task<IActionResult> Hours(string locationStub)
        {
            var viewModel = new LocationHoursViewModel();
            try
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                viewModel.LocationId = location.Id;
                viewModel.LocationName = location.Name;
                viewModel.LocationStub = location.Stub;
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Location {locationStub}: {ex.Message}");
                return RedirectToAction(nameof(LocationsController.Index));
            }

            viewModel.LocationHours = (await _locationHoursService
                .GetLocationHoursByIdAsync(viewModel.LocationId)).ToList();
            viewModel.LocationHoursOverrides = await _locationHoursService
                .GetLocationHoursOverrideByIdAsync(viewModel.LocationId);

            return View(viewModel);
        }

        [HttpPost]
        [Route("{locationStub}/[action]")]
        [SaveModelState(Key = nameof(Hours))]
        public async Task<IActionResult> Hours(LocationHoursViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            for (int i = 0; i < Enum.GetNames(typeof(DayOfWeek)).Length; i++)
            {
                var day = model.LocationHours[i];
                if (day.Open)
                {
                    if (!day.OpenTime.HasValue || !day.CloseTime.HasValue)
                    {
                        if (!day.OpenTime.HasValue)
                        {
                            ModelState.AddModelError($"LocationHours[{i}].OpenTime",
                                "Please select an Open Time.");
                        }
                        if (!day.CloseTime.HasValue)
                        {
                            ModelState.AddModelError($"LocationHours[{i}].CloseTime",
                                "Please select an Open Time.");
                        }
                    }
                    else if (day.OpenTime.Value.TimeOfDay > day.CloseTime.Value.TimeOfDay)
                    {
                        ModelState.AddModelError($"LocationHours[{i}].OpenTime",
                        "Open Time must be before the Close Time.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                await _locationHoursService.EditAsync(model.LocationHours);
                ShowAlertSuccess("Hours updated!");
            }

            return RedirectToAction(nameof(Hours), new { locationStub = model.LocationStub });
        }

        [HttpGet("[action]/{promMapPath}")]
        public async Task<IActionResult> Image(string promMapPath)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var filePath = HttpUtility.UrlDecode(promMapPath);

            var mapImagePath = Path.Combine(promBasePath,
                    ImageFilePath,
                    LocationFilePath,
                    MapFilePath,
                    Path.GetFileName(filePath));

            if (!System.IO.File.Exists(mapImagePath))
            {
                return StatusCode(404);
            }
            else
            {
                new FileExtensionContentTypeProvider()
                    .TryGetContentType(mapImagePath, out string fileType);

                return PhysicalFile(mapImagePath, fileType
                    ?? System.Net.Mime.MediaTypeNames.Application.Octet);
            }
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            return await LocationListAsync(new LocationFilter(page == 0 ? 1 : page));
        }

        [HttpGet("{locationStub}")]
        [RestoreModelState]
        public async Task<IActionResult> Location(string locationStub)
        {
            try
            {
                var location = await _locationService
                    .GetLocationByStubAsync(locationStub);
                location.IsNewLocation = false;

                var viewModel = new LocationViewModel
                {
                    Location = location,
                    LocationName = location.Name,
                    LocationStub = location.Stub,
                    LocationGroups = await _locationGroupService
                        .GetLocationGroupsByLocationAsync(location),
                    Action = nameof(LocationsController.EditLocation)
                };

                var volunteerFeature = await _featureService
                    .GetFeatureByNameAsync("Volunteer");
                if (volunteerFeature != null)
                {
                    var forms = await _volunteerFormService.GetVolunteerFormsAsync();
                    if (forms.Any())
                    {
                        var formsViewModel = new List<LocationVolunteerFormViewModel>();
                        foreach (var form in forms)
                        {
                            var mappings = await _volunteerFormService.GetFormUserMappingsAsync(form.Id, location.Id);
                            var newForm = new LocationVolunteerFormViewModel
                            {
                                TypeId = (int)form.VolunteerFormType,
                                TypeName = form.VolunteerFormType.ToString(),
                                FormMappings = mappings.ToList().ConvertAll(_ => new LocationVolunteerMappingViewModel(_)),
                                IsDisabled = form.IsDisabled
                            };
                            if (form.IsDisabled)
                            {
                                newForm.AlertWarning = $"The {form.VolunteerFormType} volunteer form is not active.";
                            }
                            formsViewModel.Add(newForm);
                        }

                        var locationFeature = await _locationFeatureService
                            .GetByIdsAsync(volunteerFeature.Id, location.Id);
                        var hasForms = formsViewModel.Any(_ => _.FormMappings.Any() && !_.IsDisabled);
                        var hasLocationFeature = locationFeature != null;

                        if (hasForms && !hasLocationFeature)
                        {
                            await _volunteerFormService
                            .AddVolunteerLocationFeature(volunteerFeature.Id, location.Id, location.Stub);
                        }
                        else if (!hasForms && hasLocationFeature)
                        {
                            await _locationFeatureService
                                .DeleteAsync(volunteerFeature.Id, location.Id);
                        }
                        viewModel.VolunteerForms = new List<LocationVolunteerFormViewModel>();
                        viewModel.VolunteerForms
                            .AddRange(formsViewModel);
                    }
                }

                viewModel.Groups = await _groupService
                    .GetGroupsByIdsAsync(viewModel.LocationGroups.Select(_ => _.GroupId));
                viewModel.LocationFeatures = await _locationFeatureService
                    .GetLocationFeaturesByLocationAsync(location);
                viewModel.Features = await _featureService
                    .GetFeaturesByIdsAsync(viewModel.LocationFeatures.Select(_ => _.FeatureId));

                var segments
                    = await _segmentService.GetNamesByIdsAsync(GetAssociatedSegmentIds(location));

                if (segments.TryGetValue(location.DescriptionSegmentId, out string value))
                {
                    viewModel.DescriptionSegmentName = value;
                }
                if (location.HoursSegmentId.HasValue)
                {
                    viewModel.HoursSegmentName = segments[location.HoursSegmentId.Value];
                }

                if (location.PostFeatureSegmentId.HasValue)
                {
                    viewModel.PostFeatureSegmentName
                        = segments[location.PostFeatureSegmentId.Value];
                }

                if (location.PreFeatureSegmentId.HasValue)
                {
                    viewModel.PreFeatureSegmentName
                        = segments[location.PreFeatureSegmentId.Value];
                }

                viewModel.FeatureList
                    = string.Join(",", viewModel.LocationFeatures.Select(_ => _.FeatureId));
                viewModel.GroupList
                    = string.Join(",", viewModel.LocationGroups.Select(_ => _.GroupId));

                if (location.SocialCardId.HasValue)
                {
                    var socialCard
                        = await _socialCardService.GetByIdAsync(location.SocialCardId.Value);
                    viewModel.SocialCardName = socialCard.Title;
                }

                return View("LocationDetails", viewModel);
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Location {locationStub}: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("{locationStub}/[action]")]
        public async Task<IActionResult> MapImageGenerator(string locationStub)
        {
            try
            {
                var location = await _locationService
                        .GetLocationByStubAsync(locationStub);
                location.IsNewLocation = false;

                var viewModel = new LocationMapViewModel
                {
                    Location = location,
                    LocationGroups = await _locationGroupService
                        .GetLocationGroupsByLocationAsync(location),
                    MapApiKey = _apiKey
                };
                return View(viewModel);
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Location {locationStub}: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]/{locationCode}")]
        public async Task<IActionResult> UpdateMapImage([FromBody] string imageBase64, string locationCode)
        {
            try
            {
                var (extension, imageBytes) = _imageService.ConvertFromBase64(imageBase64);
                var fileName = locationCode + extension;

                await _locationService.UploadLocationMapAsync(imageBytes, fileName);
                return new JsonResult("Image updated successfully!");
            }
            catch (ParameterException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        pex.Message);
            }
        }

        [HttpPost]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> MapVolunteerCoordinator(string locationStub, int type, int userId)
        {
            var location = await _locationService.GetLocationByStubAsync(locationStub);
            try
            {
                await _volunteerFormService.AddFormUserMapping(location.Id, (VolunteerFormType)type, userId);
                ShowAlertSuccess("User successfully added.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to assign user for {location.Name}: {oex.Message}");
            }
            return RedirectToAction(nameof(Location), new { locationStub });
        }

        [HttpPost]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> RemoveFormUserMapping(string locationStub, int userId, int type)
        {
            var location = await _locationService.GetLocationByStubAsync(locationStub);
            try
            {
                await _volunteerFormService.RemoveFormUserMapping(location.Id, userId, (VolunteerFormType)type);
                ShowAlertSuccess("User successfully removed.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to remove user mapping for {location.Name}: {oex.Message}");
            }
            return RedirectToAction(nameof(Location), new { locationStub });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]/{locationStub}")]
        public async Task<IActionResult> RemoveSegment(string locationStub, string whichSegment)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                ShowAlertDanger("Invalid remove segment request: no location specified.");
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(whichSegment))
            {
                ShowAlertDanger("Invalid remove segment request: no segment specified.");
                return RedirectToAction(nameof(Location), new { locationStub });
            }
            var location = await _locationService.GetLocationByStubAsync(locationStub);

            if (location == null)
            {
                ShowAlertDanger($"Location not found for stub {locationStub}.");
                return RedirectToAction(nameof(Index));
            }

            int segmentId;
            switch (whichSegment.Trim().ToUpperInvariant())
            {
                case "HOURSOVERRIDE":
                    segmentId = location.HoursSegmentId.Value;
                    location.HoursSegmentId = null;
                    break;

                case "PREFEATURE":
                    segmentId = location.PreFeatureSegmentId.Value;
                    location.PreFeatureSegmentId = null;
                    break;

                case "POSTFEATURE":
                    segmentId = location.PostFeatureSegmentId.Value;
                    location.PostFeatureSegmentId = null;
                    break;

                default:
                    ShowAlertDanger($"Invalid remove segment request: unknown segment {whichSegment}.");
                    return RedirectToAction(nameof(Location), new { locationStub });
            }

            await _locationService.EditAsync(location);

            try
            {
                await _segmentService.DeleteAsync(segmentId);
                ShowAlertSuccess("Segment removed and deleted.");
            }
            catch (OcudaException oex)
            {
                string message = oex.Message;
                if (oex.Data[OcudaExceptionData.SegmentInUseBy] is ICollection<string> inUseList)
                {
                    message = $"in use by {inUseList.Count} other locations";
                }
                ShowAlertWarning($"Segment removed from this location but not deleted: {message}");
            }

            return RedirectToAction(nameof(Location), new { locationStub });
        }

        [HttpPost]
        [Route("[action]/{locationStub}")]
        public async Task<IActionResult> RemoveSocial(string locationStub)
        {
            try
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                int? id = location.SocialCardId;
                location.SocialCardId = null;
                await _locationService.EditAsync(location);

                if (id.HasValue)
                {
                    try
                    {
                        await _socialCardService.DeleteAsync(id.Value);
                        ShowAlertSuccess("Social card deleted.");
                    }
                    catch (OcudaException oex)
                    {
                        ShowAlertWarning($"Social card unlinked from this location but could not be deleted: {oex.Message}");
                    }
                }
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Social card could not be removed: {oex.Message}");
            }

            return RedirectToAction(nameof(Location), new { locationStub });
        }

        [HttpGet]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> StructuredData(string locationStub)
        {
            try
            {
                return View(new LocationViewModel
                {
                    Location = await _locationService.GetLocationByStubAsync(locationStub),
                    LocationStub = locationStub
                });
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to find Location {locationStub}: {oex.Message}");
                return RedirectToAction(nameof(LocationsController.Index));
            }
        }

        [HttpPost]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> StructuredData(LocationViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var currentLocation
                    = await _locationService.GetLocationByStubAsync(model.LocationStub);

                currentLocation.AdministrativeArea = model.Location.AdministrativeArea?.Trim();
                currentLocation.Type = model.Location.Type?.Trim();
                currentLocation.AreaServedName = model.Location.AreaServedName?.Trim();
                currentLocation.AreaServedType = model.Location.AreaServedType?.Trim();
                currentLocation.Email = model.Location.Email?.Trim();
                currentLocation.AddressType = model.Location.AddressType?.Trim();
                currentLocation.ContactType = model.Location.ContactType?.Trim();
                currentLocation.ParentOrganization = model.Location.ParentOrganization?.Trim();
                currentLocation.IsAccessibleForFree = model.Location.IsAccessibleForFree;
                currentLocation.PriceRange = model.Location.PriceRange?.Trim();

                await _locationService.EditAsync(currentLocation);

                return RedirectToAction(nameof(StructuredData),
                    new { locationStub = model.LocationStub });
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to update structured data: {oex.Message}");
                return RedirectToAction(nameof(LocationsController.Index));
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UndeleteLocation(Location location)
        {
            ArgumentNullException.ThrowIfNull(location);

            try
            {
                await _locationService.UndeleteAsync(location.Id);
                ShowAlertSuccess($"Undeleted Location: {location.Name}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Undelete Location {location.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(LocationsController.Index));
        }

        [HttpGet]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> VolunteerForms(string locationStub)
        {
            var viewModel = new LocationFormsViewModel();
            try
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                viewModel.LocationId = location.Id;
                viewModel.LocationName = location.Name;
                viewModel.LocationStub = location.Stub;
                var volunteerTypes = _volunteerFormService.GetAllVolunteerFormTypes();
                viewModel.TypeId = null;
                viewModel.FormTypes = volunteerTypes.Select(_ => new SelectListItem
                {
                    Text = _.Key,
                    Value = _.Value.ToString(CultureInfo.InvariantCulture)
                });
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to find location {locationStub}: {oex.Message}");
                return RedirectToAction(nameof(LocationsController.Index));
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> VolunteerForms(LocationFormsViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(LocationsController.Index));
            }

            try
            {
                var location = await _locationService.GetLocationByIdAsync(viewModel.LocationId);
                viewModel.FormSubmissions = [];
                var formSubmissions = await _volunteerFormService
                    .GetVolunteerFormSubmissionsAsync(location.Id, viewModel.TypeId.Value);
                viewModel.FormSubmissions.AddRange(formSubmissions);
                var volunteerTypes = _volunteerFormService.GetAllVolunteerFormTypes();
                viewModel.FormTypes = volunteerTypes.Select(_ => new SelectListItem
                {
                    Text = _.Key,
                    Value = _.Value.ToString(CultureInfo.InvariantCulture)
                });
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to get location forms: {oex.Message}");
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpGet]
        [Route("{locationStub}/[action]")]
        public async Task<IActionResult> VolunteerFormSubmissionDetails(string locationStub, int sid)
        {
            var location = await _locationService
                .GetLocationByStubAsync(locationStub);

            try
            {
                var formSubmission = await _volunteerFormService
                    .GetVolunteerFormSubmissionAsync(sid);
                var viewModel = new VolunteerFormSubmissionDetailsViewModel(formSubmission);
                var form = await _volunteerFormService.GetFormByIdAsync(formSubmission.VolunteerFormId);
                viewModel.LocationStub = location.Stub;
                viewModel.IsTeen = form.VolunteerFormType == VolunteerFormType.Teen;
                return View(viewModel);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to retrieve information for volunteer form {sid}: {oex.Message}");
                RedirectToAction(nameof(VolunteerForms));
            }

            return RedirectToAction(nameof(Location), new { locationStub });
        }

        private static IEnumerable<int> GetAssociatedSegmentIds(Location location)
        {
            var segmentIds = new List<int> { location.DescriptionSegmentId };
            if (location.HoursSegmentId.HasValue)
            {
                segmentIds.Add(location.HoursSegmentId.Value);
            }
            if (location.PreFeatureSegmentId.HasValue)
            {
                segmentIds.Add(location.PreFeatureSegmentId.Value);
            }
            if (location.PostFeatureSegmentId.HasValue)
            {
                segmentIds.Add(location.PostFeatureSegmentId.Value);
            }
            return segmentIds.AsEnumerable();
        }

        private async Task<IActionResult> LocationListAsync(LocationFilter filter)
        {
            filter ??= new LocationFilter(1);

            filter.Take = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var locationList = await _locationService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = locationList.Count,
                CurrentPage = filter.Page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new
                {
                    page = paginateModel.LastPage ?? 1
                });
            }

            return View("Index", new LocationViewModel
            {
                AllLocations = locationList.Data,
                PaginateModel = paginateModel
            });
        }
    }
}