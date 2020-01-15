using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BranchLocator.Models;
using BranchLocator.Models.PlaceDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private readonly ILocationService _locationService;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationHoursService _locationHoursService;
        private readonly ILocationGroupService _locationGroupService;
        private readonly IFeatureService _featureService;
        private readonly IGroupService _groupService;
        private readonly IConfiguration _config;
        private readonly ISegmentService _segmentService;

        public static string Name { get { return "Locations"; } }

        public LocationsController(IConfiguration config,
            ServiceFacades.Controller<LocationsController> context,
            ILocationService locationService,
            IGroupService groupService,
            IFeatureService featureService,
            ILocationFeatureService locationFeatureService,
            ILocationHoursService locationHoursService,
            ILocationGroupService locationGroupService,
            ISegmentService segmentService) : base(context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _groupService = groupService
                ?? throw new ArgumentNullException(nameof(groupService));
            _featureService = featureService
                ?? throw new ArgumentNullException(nameof(featureService));
            _locationGroupService = locationGroupService
                ?? throw new ArgumentNullException(nameof(locationGroupService));
            _locationFeatureService = locationFeatureService
                ?? throw new ArgumentNullException(nameof(locationFeatureService));
            _locationHoursService = locationHoursService
                ?? throw new ArgumentNullException(nameof(locationHoursService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var locationList = await _locationService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = locationList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new
                {
                    page = paginateModel.LastPage ?? 1
                });
            }

            return View(new LocationViewModel
            {
                AllLocations = locationList.Data,
                PaginateModel = paginateModel
            });
        }

        [HttpGet("{locationStub}")]
        [RestoreModelState]
        public async Task<IActionResult> Location(string locationStub)
        {
            try
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                location.IsNewLocation = false;
                var viewModel = new LocationViewModel
                {
                    Location = location,
                    LocationFeatures = await _locationFeatureService.GetLocationFeaturesByLocationAsync(location),
                    LocationGroups = await _locationGroupService.GetLocationGroupsByLocationAsync(location),
                    Groups = await _groupService.GetAllGroupsAsync(),
                    Features = await _featureService.GetAllFeaturesAsync(),
                    Action = nameof(LocationsController.EditLocation),
                    AllLocationHours = await _locationHoursService.GetLocationHoursByIdAsync(location.Id)
                };
                var segments = await _segmentService.GetActiveSegmentsAsync();
                viewModel.PostFeatSegments = new SelectList(segments, nameof(Segment.Id),
                    nameof(Segment.Name),viewModel.Location?.PostFeatureSegmentId);
                viewModel.PreFeatSegments = new SelectList(segments, nameof(Segment.Id),
                    nameof(Segment.Name), viewModel.Location?.PreFeatureSegmentId);
                viewModel.Location.LocationHours = await _locationService.GetFormattedWeeklyHoursAsync(viewModel.Location.Id);
                viewModel.FeatureList = string.Join(",", viewModel.LocationFeatures.Select(_ => _.FeatureId));
                viewModel.GroupList = string.Join(",", viewModel.LocationGroups.Select(_ => _.GroupId));
                return View("LocationDetails", viewModel);
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Location {locationStub}: {ex.Message}");
                return RedirectToAction(nameof(LocationsController.Index));
            }
        }

        [HttpGet("[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddLocation()
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
            var segments = await _segmentService.GetActiveSegmentsAsync();
            viewModel.PostFeatSegments = new SelectList(segments, nameof(Segment.Id),
                nameof(Segment.Name), viewModel.Location?.PostFeatureSegmentId);
            viewModel.PreFeatSegments = new SelectList(segments, nameof(Segment.Id),
                nameof(Segment.Name), viewModel.Location?.PreFeatureSegmentId);
            return View("LocationDetails", viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateLocation(Location location)
        {
            if (ModelState.IsValid)
            {
                if (location.Phone.Length == 10)
                {
                    location.Phone = string.Format("+1 {0:###-###-####}", Convert.ToInt64(location.Phone));
                }
                try
                {
                    location = await GetLatLng(location);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Problem looking up postal code for coordinates {Address}: {Message}",
                        location.Address,
                        ex.Message);
                    ShowAlertDanger($"Unable to find Location's address: {location.Address}");
                    location.IsNewLocation = true;
                    return View("LocationDetails", new LocationViewModel
                    {
                        Location = location,
                        Action = nameof(LocationsController.CreateLocation)
                    });
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
                    return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
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
        public async Task<IActionResult> DeleteLocation(Location location)
        {
            try
            {
                var features = await _locationFeatureService.GetLocationFeaturesByLocationAsync(location);
                var groups = await _locationGroupService.GetLocationGroupsByLocationAsync(location);
                if (groups.Count > 0 || features.Count > 0)
                {
                    ShowAlertDanger($"You must delete all features and groups from {location.Name} before deleting it");
                    return RedirectToAction(nameof(Index));
                }

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
        public async Task<IActionResult> EditLocation(Location location)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var oldLocation = await _locationService.GetLocationByIdAsync(location.Id);
                    if (!(oldLocation.Address.Equals(location.Address) && oldLocation.City.Equals(location.City)
                        && oldLocation.State.Equals(location.State) && oldLocation.Zip.Equals(location.Zip))
                        && oldLocation.Country.Equals(location.Country))
                    {
                        try
                        {
                            location = await GetLatLng(location);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Problem looking up postal code for coordinates {Address}: {Message}",
                                location.Address,
                                ex.Message);
                            ShowAlertDanger($"Unable to find Location's address: {location.Address}");
                            location.IsNewLocation = true;
                            location.LocationHours = await _locationService.GetFormattedWeeklyHoursAsync(location.Id);
                            return View("LocationDetails", new LocationViewModel
                            {
                                Location = location,
                                Action = nameof(LocationsController.EditLocation)
                            });
                        }
                    }

                    await _locationService.EditAsync(location);
                    ShowAlertSuccess($"Updated Location: {location.Name}");
                    location.IsNewLocation = false;
                    return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update Location {location.Name}: {ex.Message}");
                    _logger.LogError(ex,
                        "Problem updating location {LocationName}: {Message}",
                        location.Name,
                        ex.Message);
                    location.IsNewLocation = false;
                    location.LocationHours = await _locationService.GetFormattedWeeklyHoursAsync(location.Id);
                    var viewModel = new LocationViewModel
                    {
                        Location = location,
                        Action = nameof(LocationsController.EditLocation)
                    };

                    return View("LocationDetails", viewModel);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditLocationGroup(LocationViewModel locationGroupInfo)
        {
            var location = await _locationService.GetLocationByIdAsync(locationGroupInfo.LocationGroup.LocationId);
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
                    var group = await _groupService.GetGroupByIdAsync(locationGroupInfo.LocationGroup.GroupId);
                    ShowAlertSuccess($"Updated {location.Name}'s Group: {group.GroupType}");
                }
                catch (OcudaException ex)
                {
                    var group = await _groupService.GetGroupByIdAsync(locationGroupInfo.LocationGroup.GroupId);
                    ShowAlertDanger($"problem Updating {location.Name}'s Group: {group.GroupType}");
                    _logger.LogError(ex, "Problem updating group {Group} for location {LocationName}: {Message}",
                        group.GroupType,
                        location.Name,
                        ex.Message);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditLocationFeature(LocationFeature locationFeature)
        {
            var location = await _locationService.GetLocationByIdAsync(locationFeature.LocationId);
            if (ModelState.IsValid)
            {
                try
                {
                    await _locationFeatureService.EditAsync(locationFeature);
                    var feature = await _featureService.GetFeatureByIdAsync(locationFeature.FeatureId);
                    ShowAlertSuccess($"Updated {location.Name}'s Feature: {feature.Name}");
                }
                catch (OcudaException ex)
                {
                    var feature = await _featureService.GetFeatureByIdAsync(locationFeature.FeatureId);
                    ShowAlertDanger($"Failed to Update {location.Name}'s Feature: {feature.Name}");
                    _logger.LogError(ex,
                        "Unable to edit feature {FeatureName} for location {LocationName}: {Message}",
                        feature.Name,
                        location.Name,
                        ex.Message);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditLocationHours(LocationViewModel viewModel)
        {
            var location = await _locationService.GetLocationByIdAsync(viewModel.AllLocationHours[0].LocationId);
            var updateAlwaysOpen = true;
            foreach (var hour in viewModel.AllLocationHours)
            {
                if (hour.Open && (hour.OpenTime == null || hour.CloseTime == null))
                {
                    ShowAlertDanger($"Location hours must be provided if open");
                    return RedirectToAction(nameof(Location), new { locationStub = location.Stub });
                }
                if (hour.CloseTime.HasValue && hour.OpenTime.HasValue)
                {
                    if (DateTime.Compare(hour.CloseTime.Value, hour.OpenTime.Value) < 0)
                    {
                        ShowAlertDanger($"Location can't close before it's opening time");
                        return RedirectToAction(nameof(Location), new { locationStub = location.Stub });
                    }
                    if (DateTime.Compare(hour.CloseTime.Value, hour.OpenTime.Value) == 0)
                    {
                        ShowAlertDanger($"Location opening and closing times can't be equal.");
                        return RedirectToAction(nameof(Location), new { locationStub = location.Stub });
                    }
                }
                if (hour.Open)
                {
                    updateAlwaysOpen = false;
                }
            }
            try
            {
                location.IsAlwaysOpen = updateAlwaysOpen;
                await _locationService.EditAlwaysOpenAsync(location);
                ShowAlertSuccess($"Updated {location.Name}'s Hours");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Failed to Update {location.Name}'s Hours");
                _logger.LogError(ex,
                    "Unable to edit hours for {LocationName}: {Message}",
                    location.Name,
                    ex.Message);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _locationHoursService.EditAsync(viewModel.AllLocationHours);
                    ShowAlertSuccess($"Updated {location.Name}'s Hours");
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Failed to Update {location.Name}'s Hours");
                    _logger.LogError(ex,
                        "Unable to edit hours for {LocationName}: {Message}",
                        location.Name,
                        ex.Message);
                }
            }
            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
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

            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
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

            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
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

            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
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
            return RedirectToAction(nameof(LocationsController.Location), new { locationStub = location.Stub });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetItemsList(string itemIds, string objectType, int page = 1)
        {
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
                viewModel.Group = await _groupService.GetGroupByIdAsync(viewModel.LocationGroup.GroupId);
                viewModel.IsLocationsGroup = location.DisplayGroupId == viewModel.Group.Id ? true : false;
                return PartialView("_EditGroupsPartial", viewModel);
            }
            else if (objectType == "Feature")
            {
                viewModel.LocationFeature = await _locationFeatureService
                    .GetByIdsAsync(itemId, location.Id);
                viewModel.Features = await _featureService.GetAllFeaturesAsync();
                return PartialView("_EditFeaturesPartial", viewModel);
            }
            else
            {
                viewModel.AllLocationHours = await _locationHoursService.GetLocationHoursByIdAsync(location.Id);
                return PartialView("_EditHoursPartial", viewModel);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetUrlLocations(string addressJson)
        {
            bool success = false;
            var apikey = _config[Configuration.OpsAPIGoogleMaps];
            var location = JsonConvert
                .DeserializeObject<Location>(addressJson);
            string addrstr = "";
            if (!string.IsNullOrEmpty(location.Address))
            {
                addrstr += location.Address;
            }
            if (!string.IsNullOrEmpty(location.City))
            {
                addrstr += "," + location.City;
            }
            if (!string.IsNullOrEmpty(location.State))
            {
                addrstr += "," + location.State;
            }
            if (!string.IsNullOrEmpty(location.Zip))
            {
                addrstr += "," + location.Zip;
            }
            try
            {
                using var client = new HttpClient();
                GeocodePlace geoPlace = null;
                string stringResult = null;
                try
                {
                    var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/textsearch/json?query=establishment+in+{addrstr}&key={apikey}");
                    response.EnsureSuccessStatusCode();

                    stringResult = await response.Content.ReadAsStringAsync();

                    geoPlace = JsonConvert.DeserializeObject<GeocodePlace>(stringResult);
                    var results = new List<PlaceDetailsResult>();
                    foreach (var result in geoPlace.Results.Where(_ => !_.PlaceId.Equals("") || _.PlaceId != null))
                    {
                        string stringDetailResult = null;

                        var detailResponse = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/details/json?placeid={result.PlaceId}&key={apikey}");
                        detailResponse.EnsureSuccessStatusCode();

                        stringDetailResult = await detailResponse.Content.ReadAsStringAsync();
                        var geoDetailPlace = JsonConvert.DeserializeObject<GeocodePlaceDetails>(stringDetailResult);
                        if (geoDetailPlace != null)
                        {
                            results.Add(geoDetailPlace.Results);
                        }
                    }
                    string data = JsonConvert.SerializeObject(results);
                    success = true;
                    return Json(new
                    {
                        success,
                        data
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing Geocode API JSON: {Message} - {Result}",
                        ex.Message,
                        stringResult);
                    return Json(new
                    {
                        success
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Problem looking up postal code for {LocationAddress}: {Message}",
                    location.Address,
                    ex.Message);
                return Json(new
                {
                    success
                });
            }
        }

        public async Task<Location> GetLatLng(Location location)
        {
            try
            {
                using var client = new HttpClient();
                var apikey = _config[Configuration.OpsAPIGoogleMaps];

                GeocodeResult geoResult = null;
                string stringResult = null;
                try
                {
                    var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={location.Address},{location.City},{location.State}&key={apikey}");
                    response.EnsureSuccessStatusCode();

                    stringResult = await response.Content.ReadAsStringAsync();

                    geoResult = JsonConvert.DeserializeObject<GeocodeResult>(stringResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing Geocode API JSON: {Message} - {Result}",
                        ex.Message,
                        stringResult);
                }

                if (geoResult?.Results?.Count() > 0)
                {
                    var lat = geoResult?
                        .Results?
                        .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                        .Geometry?
                        .Location?
                        .Lat;
                    var lng = geoResult?
                        .Results?
                        .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                        .Geometry?
                        .Location?
                        .Lng;

                    if (lat != null && lng != null)
                    {
                        location.GeoLocation = lat.ToString() + "," + lng.ToString();
                    }
                    else
                    {
                        _logger.LogInformation("Could not find latitude and longitude when geocoding {LocationAddress}",
                            location.Address);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Problem looking up postal code for {LocationAddress}: {Message}",
                    location.Address,
                    ex.Message);
            }
            return location;
        }
    }
}
