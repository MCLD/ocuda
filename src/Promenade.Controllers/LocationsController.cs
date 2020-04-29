using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BranchLocator.Helpers;
using BranchLocator.Models;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Locations;
using Ocuda.Promenade.Service;
using Serilog.Context;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private const string MapQuery
            = "https://maps.googleapis.com/maps/api/geocode/json?{0}={1}&key={2}";

        private readonly LocationService _locationService;
        private readonly SegmentService _segmentService;

        private readonly string ApiKey;

        public static string Name { get { return "Locations"; } }

        public IFormatProvider CurrentCulture { get; private set; }

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            LocationService locationService, SegmentService segmentService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));

            ApiKey = _config[Utility.Keys.Configuration.PromenadeAPIGoogleMaps];
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        [HttpGet("[action]/{Zip}")]
        [HttpGet("[action]/{latitude}/{longitude}")]
        public async Task<IActionResult> Find(string zip, double? latitude, double? longitude)
        {
            if (!string.IsNullOrEmpty(zip))
            {
                return await FindAsync(zip);
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                return await FindAsync(latitude, longitude);
            }
            else
            {
                var viewModel = await CreateLocationViewModelAsync();
                return await ShowNearestAsync(viewModel);
            }
        }

        private async Task<string> PeformMapQueryAsync(string query)
        {
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(new Uri(query));
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                using (LogContext.PushProperty(Utility.Logging.Enrichment.APIQuery, query))
                {
                    _logger.LogWarning(ex,
                        "Error with map API query: {ErrorMessage}",
                        ex.Message);
                }
                TempData["AlertDanger"] = "An error occured, please try again later.";
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return null;
        }

        private async Task<LocationViewModel> CreateLocationViewModelAsync()
        {
            var viewModel = new LocationViewModel
            {
                Locations = await _locationService.GetAllLocationsAsync(),
                CanSearchAddress = !string.IsNullOrWhiteSpace(ApiKey)
            };

            foreach (var location in viewModel.Locations)
            {
                location.CurrentStatus = await _locationService.GetCurrentStatusAsync(location.Id);
            }

            return viewModel;
        }

        private async Task<IActionResult> FindAsync(string zip)
        {
            var viewModel = await CreateLocationViewModelAsync();

            if (viewModel.CanSearchAddress && !string.IsNullOrEmpty(zip))
            {
                var mapQueryResult
                    = await PeformMapQueryAsync(string.Format(CultureInfo.InvariantCulture,
                    MapQuery,
                    "address",
                    zip.Trim(),
                    ApiKey));

                if (!string.IsNullOrEmpty(mapQueryResult))
                {
                    try
                    {
                        dynamic jsonResult = JsonConvert.DeserializeObject(mapQueryResult);

                        if (jsonResult.results.Count > 0)
                        {
                            viewModel.Latitude = jsonResult.results[0].geometry.location.lat;
                            viewModel.Longitude = jsonResult.results[0].geometry.location.lng;
                            viewModel.Zip = zip.Trim();
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
                    {
                        using (LogContext.PushProperty(Utility.Logging.Enrichment.APIResult,
                            mapQueryResult))
                        {
                            _logger.LogWarning(ex,
                                "Could not parse address API result into JSON: {ErrorMessage}",
                                ex.Message);
                        }
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
            }

            return await ShowNearestAsync(viewModel);
        }

        private async Task<IActionResult> FindAsync(double? latitude, double? longitude)
        {
            var viewModel = await CreateLocationViewModelAsync();

            if (viewModel.CanSearchAddress
                && latitude.HasValue
                && longitude.HasValue)
            {
                viewModel.Latitude = latitude;
                viewModel.Longitude = longitude;

                var mapQueryResult
                    = await PeformMapQueryAsync(string.Format(CultureInfo.InvariantCulture,
                        MapQuery,
                        "latlng",
                        $"{viewModel.Latitude},{viewModel.Longitude}",
                        ApiKey));

                if (!string.IsNullOrEmpty(mapQueryResult))
                {
                    try
                    {
                        var geoResult = JsonConvert.DeserializeObject<GeocodeResult>(mapQueryResult);

                        if (geoResult?.Results?.Count() > 0)
                        {
                            var zipCode = geoResult?
                                .Results?
                                .FirstOrDefault(_ => _.Types.Any(__ => __ == "postal_code"))?
                                .AddressComponents?
                                .FirstOrDefault()?
                                .ShortName;

                            if (string.IsNullOrEmpty(zipCode))
                            {
                                _logger.LogWarning("Could not find postal code when reverse geocoding {Coordinates}",
                                    $"{viewModel.Latitude},{viewModel.Longitude}");
                            }
                            else
                            {
                                viewModel.Zip = zipCode;
                            }
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
                    {
                        using (LogContext.PushProperty(Utility.Logging.Enrichment.APIResult,
                            mapQueryResult))
                        {
                            _logger.LogWarning(ex,
                                "Could not parse latlng API result into JSON: {ErrorMessage}",
                                ex.Message);
                        }
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
            }

            return await ShowNearestAsync(viewModel);
        }

        private async Task<IActionResult> ShowNearestAsync(LocationViewModel viewModel)
        {
            if (viewModel == null)
            {
                viewModel = await CreateLocationViewModelAsync();
            }

            if (viewModel.Latitude.HasValue && viewModel.Longitude.HasValue)
            {
                foreach (var location in viewModel.Locations)
                {
                    var geolocation = location.GeoLocation
                        .Split(',')
                        .Select(Convert.ToDouble).ToList();
                    location.Distance = HaversineHelper.Calculate(
                        geolocation[0],
                        geolocation[1],
                        (double)viewModel.Latitude,
                        (double)viewModel.Longitude);
                }

                viewModel.Locations = viewModel
                    .Locations
                    .OrderBy(_ => _.Distance)
                    .Select(_ =>
                    {
                        _.Distance = Math.Ceiling(_.Distance);
                        return _;
                    })
                    .ToList();
            }

            PageTitle = "Find my library";

            return View("Locations", viewModel);
        }

        [HttpGet("{locationStub}")]
        public async Task<IActionResult> Locations(string locationStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return RedirectToAction(nameof(Find));
            }

            var viewModel = new LocationDetailViewModel
            {
                CanonicalUrl = await GetCanonicalUrl(),
                Location = await _locationService.GetLocationByStubAsync(locationStub)
            };

            if (viewModel.Location == null)
            {
                return NotFound();
            }

            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            if (viewModel.Location.PreFeatureSegmentId.HasValue)
            {
                viewModel.PreFeatureSegment = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    viewModel.Location.PreFeatureSegmentId.Value, forceReload);
                if (viewModel.PreFeatureSegment != null)
                {
                    viewModel.PreFeatureSegment.Text
                        = CommonMarkConverter.Convert(viewModel.PreFeatureSegment.Text);
                }
            }

            if (viewModel.Location.PostFeatureSegmentId.HasValue)
            {
                viewModel.PostFeatureSegment = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    viewModel.Location.PostFeatureSegmentId.Value, forceReload);
                if (viewModel.PostFeatureSegment != null)
                {
                    viewModel.PostFeatureSegment.Text
                        = CommonMarkConverter.Convert(viewModel.PostFeatureSegment.Text);
                }
            }

            viewModel.Location.DescriptionSegment = await _segmentService
                .GetSegmentTextBySegmentIdAsync(viewModel.Location.DescriptionSegmentId,
                    forceReload);

            if (viewModel.Location.DescriptionSegment?.Text.Length > 0)
            {
                viewModel.Location.DescriptionSegment.Text = CommonMarkConverter
                    .Convert(viewModel.Location.DescriptionSegment.Text);
            }

            viewModel.Location.LocationHours
                = await _locationService.GetFormattedWeeklyHoursAsync(viewModel.Location.Id);

            if (viewModel.Location.LocationHours != null)
            {
                viewModel.StructuredLocationHours
                    = (await _locationService.GetFormattedWeeklyHoursAsync(viewModel.Location.Id, true))
                    .Select(_ => $"{_.Days} {_.Time}").ToList();
            }

            var locationFeatures
                = await _locationService.GetFullLocationFeaturesAsync(locationStub);

            viewModel.LocationFeatures = locationFeatures
                .Select(_ => new LocationsFeaturesViewModel(_));

            if (viewModel.Location.DisplayGroupId.HasValue)
            {
                viewModel.LocationNeighborGroup = await _locationService
                    .GetLocationsNeighborGroup(viewModel.Location.DisplayGroupId.Value);

                var neighbors = await _locationService
                    .GetLocationsNeighborsAsync(viewModel.Location.DisplayGroupId.Value);
                if (neighbors.Count > 0)
                {
                    viewModel.NearbyLocationGroups = neighbors;
                    viewModel.NearbyCount = viewModel.NearbyLocationGroups.Count;
                    viewModel.NearbyEventsCount = viewModel.NearbyLocationGroups
                        .Count(_ => _.Location.HasEvents);
                }
            }

            PageTitle = viewModel.Location.Name;

            return View("LocationDetails", viewModel);
        }

        [HttpGet("{locationStub}/{featureStub}")]
        public async Task<IActionResult> Locations(string locationStub, string featureStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return RedirectToAction(nameof(Find));
            }

            var locationFeature
                = await _locationService.GetLocationFullFeatureAsync(locationStub, featureStub);

            if (locationFeature?.Feature != null)
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                PageTitle = $"{locationFeature.Feature.Name} at {location.Name} Library";

                return View("LocationFeatureDetails", new LocationDetailViewModel
                {
                    CanonicalUrl = await GetCanonicalUrl(),
                    LocationFeatures = new List<LocationsFeaturesViewModel>
                    {
                        new LocationsFeaturesViewModel(locationFeature)
                    },
                    Location = location
                });
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{locationStub}/[action]/{featureStub}")]
        public async Task<IActionResult> FeatureInfo(string locationStub, string featureStub)
        {
            var locationFeature
                 = await _locationService.GetLocationFullFeatureAsync(locationStub, featureStub);

            if (locationFeature != null)
            {
                var viewModel = new FeatureInfoViewModel
                {
                    BodyText = CommonMarkConverter.Convert(locationFeature.Feature.BodyText),
                    Text = CommonMarkConverter.Convert(locationFeature.Text)
                };

                return Json(viewModel);
            }
            else
            {
                _logger.LogWarning("Location Feature not found for location {locationStub} and feature {featureStub}",
                        locationStub,
                        featureStub);
                return NotFound();
            }
        }
    }
}
