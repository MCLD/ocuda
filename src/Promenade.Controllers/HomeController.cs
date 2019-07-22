using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BranchLocator.Helpers;
using BranchLocator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]

    public class HomeController : BaseController
    {
        public static readonly int DaysInAWeek = 7;
        private readonly IConfiguration _config;
        private readonly LocationService _locationService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration config,
            LocationService locationService,
            ILogger<HomeController> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{zip}")]
        [HttpGet("[action]/{latitude}/{longitude}")]
        public async Task<IActionResult> Find(double latitude = 0, double longitude = 0, string zip = null)
        {
            var viewModel = new Location();

            if (!string.IsNullOrWhiteSpace(zip) && (latitude.Equals(0) && longitude.Equals(0)))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        var apikey = _config[Ocuda.Utility.Keys.Configuration.PromAPIGoogleMaps];

                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={zip}&key={apikey}");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        dynamic jsonResult = JsonConvert.DeserializeObject(stringResult);

                        if (jsonResult.results.Count > 0)
                        {
                            var result = jsonResult.results[0];
                            var newLat = result.geometry.location.lat;
                            var newLong = result.geometry.location.lng;

                            viewModel = LookupLocation(newLat, newLong);
                            viewModel.FormattedAddress = result.formatted_address;

                            return View("Locations", viewModel);
                        }
                        else
                        {
                            _logger.LogError($"No geocoding results for a \"{zip}\"");
                            TempData["AlertDanger"] = $"Unable to locate zip <strong>\"{zip}\"</strong>.";
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogCritical(ex, $"Google API error: {ex.Message}");
                        TempData["AlertDanger"] = "An error occured, please try again later.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, ex.Message);
                        TempData["AlertDanger"] = "An error occured, please try again later.";
                    }
                }
                viewModel.CloseLocations = (await _locationService.GetAllLocationsAsync()).OrderBy(c => c.Name).ToList();
                return View("Locations", viewModel);
            }
            else if (!latitude.Equals(0) && !longitude.Equals(0) && string.IsNullOrEmpty(zip))
            {
                viewModel = LookupLocation(latitude, longitude);
                var latlng = $"{latitude},{longitude}";

                // try to get the zip code to display to the user
                try
                {
                    using (var client = new HttpClient())
                    {
                        var apikey = _config[Ocuda.Utility.Keys.Configuration.PromAPIGoogleMaps];

                        GeocodeResult geoResult = null;
                        string stringResult = null;

                        try
                        {
                            var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?latlng={latlng}&key={apikey}");
                            response.EnsureSuccessStatusCode();

                            stringResult = await response.Content.ReadAsStringAsync();

                            geoResult = JsonConvert.DeserializeObject<GeocodeResult>(stringResult);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error parsing Geocode API JSON: {ex.Message} - {stringResult}");
                        }

                        if (geoResult?.Results?.Count() > 0)
                        {
                            viewModel.Address = geoResult.Results?
                                .FirstOrDefault(_ => _.Types.Any(__ => __ == "postal_code"))?
                                .AddressComponents?
                                .FirstOrDefault()?
                                .ShortName;
                            if (string.IsNullOrEmpty(viewModel.Address))
                            {
                                _logger.LogInformation($"Could not find postal code when geocoding {latlng}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Problem looking up postal code for coordinates {latlng}: {ex.Message}");
                }
                return View("Locations", viewModel);
            }
            else
            {
                return View("Locations", new Location
                {
                    CloseLocations = (await _locationService.GetAllLocationsAsync()).OrderBy(c => c.Name).ToList()
                });
            }
        }

        [HttpGet("[action]/{locationStub}")]
        [HttpGet("[action]/{locationStub}/{featureStub}")]
        public async Task<IActionResult> LocationsAsync(string locationStub, string featureStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return View("Locations", await _locationService.GetAllLocationsAsync());
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                var locationViewModel = new LocationViewModel();
                var locationFeatureViewModel = new List<LocationsFeaturesViewModel>();
                locationViewModel.Location = await _locationService.GetLocationByStubAsync(locationStub);
                var features = await _locationService.GetLocationsFeaturesAsync(locationStub);

                foreach (var feature in features)
                {
                    var locationFeature = await _locationService.GetLocationFeatureByFeatureId(feature.Id);
                    var locationfeatureModel = new LocationsFeaturesViewModel
                    {
                        BodyText = feature.BodyText,
                        FontAwesome = feature.FontAwesome,
                        ImagePath = feature.ImagePath,
                        Name = feature.Name,
                        RedirectUrl = locationFeature.RedirectUrl,
                        Stub = feature.Stub,
                        Text = locationFeature.Text
                    };
                    locationFeatureViewModel.Add(locationfeatureModel);
                }
                locationViewModel.LocationFeatures = locationFeatureViewModel;
                locationViewModel.NearbyLocations = await _locationService.GetLocationsNeighborsAsync(locationStub);
                locationViewModel.NearbyCount = locationViewModel.NearbyLocations.Count;
                return View("LocationDetails", locationViewModel);
            }
            else
            {
                var locationViewModel = new LocationViewModel();
                var locationFeatureViewModel = new List<LocationsFeaturesViewModel>();

                locationViewModel.Location = await _locationService.GetLocationByStubAsync(locationStub);
                var feature = (Feature)(await _locationService.GetLocationsFeaturesAsync(locationStub)).Where(_ => _.Stub == featureStub);
                var locationFeature = await _locationService.GetLocationFeatureByFeatureId(feature.Id);

                var locationfeatureModel = new LocationsFeaturesViewModel
                {
                    BodyText = feature.BodyText,
                    FontAwesome = feature.FontAwesome,
                    ImagePath = feature.ImagePath,
                    Name = feature.Name,
                    RedirectUrl = locationFeature.RedirectUrl,
                    Stub = feature.Stub,
                    Text = locationFeature.Text
                };

                locationFeatureViewModel.Add(locationfeatureModel);
                locationViewModel.LocationFeatures = locationFeatureViewModel;

                return View("LocationFeatureDetails", locationViewModel);
            }
        }

        [NonAction]
        public static Location LookupLocation(double latitude, double longitude)
        {
            var viewModel = new Location();
            Location[] locations = { };

            foreach (var location in locations)
            {
                var geolocation = location.GeoLocation
                    .Split(',')
                    .Select(_ => Convert.ToDouble(_)).ToList();
                location.Distance = HaversineHelper
                    .Calculate(geolocation[0], geolocation[1], latitude, longitude);
                location.MapLink = $"https://maps.googleapis.com/maps/api/staticmap?center={latitude},{longitude}&zoom=12&maptype=roadmap&format=png&visual_refresh=true";
            }
            viewModel.CloseLocations = locations.OrderBy(_ => _.Distance).ToList();
            viewModel.CloseLocations = viewModel.CloseLocations.Select(_ =>
            {
                _.Distance = Math.Ceiling(_.Distance);
                return _;
            }).ToList();

            return viewModel;
        }
    }
}
