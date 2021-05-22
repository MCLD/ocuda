using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BranchLocator.Models;
using BranchLocator.Models.PlaceDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _config;
        private readonly IFeatureService _featureService;
        private readonly IGroupService _groupService;
        private readonly ILanguageService _languageService;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationGroupService _locationGroupService;
        private readonly ILocationHoursService _locationHoursService;
        private readonly ILocationService _locationService;
        private readonly ISegmentService _segmentService;
        private readonly ISocialCardService _socialCardService;

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
            ISocialCardService socialCardService) : base(context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _featureService = featureService
                ?? throw new ArgumentNullException(nameof(featureService));
            _groupService = groupService
                ?? throw new ArgumentNullException(nameof(groupService));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _locationFeatureService = locationFeatureService
                ?? throw new ArgumentNullException(nameof(locationFeatureService));
            _locationGroupService = locationGroupService
                ?? throw new ArgumentNullException(nameof(locationGroupService));
            _locationHoursService = locationHoursService
                ?? throw new ArgumentNullException(nameof(locationHoursService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "Locations"; } }

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

            return View("LocationDetails", viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState(Key = nameof(Hours))]
        public async Task<IActionResult> AddOverride(LocationHoursViewModel model)
        {
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
                            "Please select an Close Time.");
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
                var inUseList = oex.Data[OcudaExceptionData.SegmentInUseBy] as ICollection<string>;
                if (inUseList != null)
                {
                    message = $"in use by {inUseList.Count} other locations";
                }
                ShowAlertWarning($"Segment removed from this location but not deleted: {message}");
            }

            return RedirectToAction(nameof(Location), new { locationStub });
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
                { "PostFeature", "Post-feature"}
            };

            if (!validSegments.ContainsKey(whichSegment))
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
                Name = $"{location.Name} - {validSegments[whichSegment]}",
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
                case "DESCRIPTION":
                    location.DescriptionSegmentId = segment.Id;
                    break;

                case "HOURSOVERRIDE":
                    location.HoursSegmentId = segment.Id;
                    break;

                case "PREFEATURE":
                    location.PreFeatureSegmentId = segment.Id;
                    break;

                case "POSTFEATURE":
                    location.PostFeatureSegmentId = segment.Id;
                    break;
            }

            await _locationService.EditAsync(location);

            return RedirectToAction(nameof(Location), new { locationStub });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]/{locationStub}")]
        public async Task<IActionResult> AddSocial(string locationStub, string whichSegment)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateLocation(Location location)
        {
            if (ModelState.IsValid)
            {
                if (location.Phone?.Length == 10)
                {
                    location.Phone = $"+1 {Convert.ToInt64(location.Phone):###-###-####}";
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

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteLocation(Location location)
        {
            try
            {
                var features = await _locationFeatureService
                    .GetLocationFeaturesByLocationAsync(location);
                var groups = await _locationGroupService
                    .GetLocationGroupsByLocationAsync(location);

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
            try
            {
                await _locationHoursService.DeleteLocationsHoursOverrideAsync(
                    model.DeleteOverride.Id);
                ShowAlertSuccess($"Deleted override: {model.DeleteOverride.Reason}");
            }
            catch (Exception ex)
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
            if (ModelState.IsValid)
            {
                try
                {
                    var currentLocation = await _locationService.GetLocationByIdAsync(location.Id);

                    var locationHasChanged = currentLocation.Address != location.Address
                        || currentLocation.City != location.City
                        || currentLocation.State != location.State
                        || currentLocation.Zip != location.Zip
                        || currentLocation.Country != location.Country;

                    var hasLocation = !(string.IsNullOrEmpty(location.Address)
                        && string.IsNullOrEmpty(location.City)
                        && string.IsNullOrEmpty(location.State)
                        && string.IsNullOrEmpty(location.Zip)
                        && string.IsNullOrEmpty(location.Country));

                    if (locationHasChanged && hasLocation)
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
                            location.LocationHours = await _locationService
                                .GetFormattedWeeklyHoursAsync(location.Id);
                            return View("LocationDetails", new LocationViewModel
                            {
                                Location = location,
                                Action = nameof(LocationsController.EditLocation)
                            });
                        }
                    }

                    currentLocation.Address = location.Address?.Trim();
                    currentLocation.City = location.City?.Trim();
                    currentLocation.Code = location.Code?.Trim();
                    currentLocation.EventLink = location.EventLink?.Trim();
                    currentLocation.Facebook = location.Facebook?.Trim();
                    currentLocation.MapLink = location.MapLink?.Trim();
                    currentLocation.Name = location.Name?.Trim();
                    currentLocation.Phone = location.Phone?.Trim();
                    currentLocation.State = location.State?.Trim();
                    currentLocation.Stub = location.Stub?.Trim();
                    currentLocation.SubscriptionLink = location.SubscriptionLink?.Trim();
                    currentLocation.Zip = location.Zip?.Trim();

                    await _locationService.EditAsync(location);
                    ShowAlertSuccess($"Updated Location: {location.Name}");
                    location.IsNewLocation = false;
                    return RedirectToAction(nameof(LocationsController.Location),
                        new { locationStub = location.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update Location {location.Name}: {ex.Message}");
                    _logger.LogError(ex,
                        "Problem updating location {LocationName}: {Message}",
                        location.Name,
                        ex.Message);
                    location.IsNewLocation = false;
                    location.LocationHours = await _locationService
                        .GetFormattedWeeklyHoursAsync(location.Id);
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
                            "Please select an Close Time.");
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
            int page = 1)
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
                    LocationName = location.Name,
                    LocationStub = location.Stub,
                    LocationFeatures = await _locationFeatureService
                        .GetLocationFeaturesByLocationAsync(location),
                    LocationGroups = await _locationGroupService
                        .GetLocationGroupsByLocationAsync(location),
                    Action = nameof(LocationsController.EditLocation)
                };

                viewModel.Features = await _featureService
                    .GetFeaturesByIdsAsync(viewModel.LocationFeatures.Select(_ => _.FeatureId));

                viewModel.Groups = await _groupService
                    .GetGroupsByIdsAsync(viewModel.LocationGroups.Select(_ => _.GroupId));

                var segments
                    = await _segmentService.GetNamesByIdsAsync(GetAssociatedSegmentIds(location));

                if (segments.ContainsKey(location.DescriptionSegmentId))
                {
                    viewModel.DescriptionSegmentName = segments[location.DescriptionSegmentId];
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
                return RedirectToAction(nameof(LocationsController.Index));
            }
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
    }
}