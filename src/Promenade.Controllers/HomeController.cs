using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Models.Entities;
using BranchLocator.Helpers;
using BranchLocator.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]

    public class HomeController : BaseController
    {
        public const int DaysInAWeek = 7;

        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration config,
            ILogger<HomeController> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var features = new List<Feature> { };

            var locationFeatures = new List<LocationFeature> { };

            Location[] locations = { };

            var viewModel = new Location();

            if (!string.IsNullOrWhiteSpace(zip) && (latitude == 0 && longitude == 0))
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
                            latitude = result.geometry.location.lat;
                            longitude = result.geometry.location.lng;

                            viewModel = LookupLocation(latitude, longitude);
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
                viewModel.CloseLocations = locations.OrderBy(c => c.Name).ToList();
                return View("Locations", viewModel);
            }
            else if (latitude != 0 && longitude != 0 && string.IsNullOrEmpty(zip))
            {
                viewModel = LookupLocation(latitude, longitude);
                viewModel.ShowLocation = true;
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
                                .FirstOrDefault(_ => _.Types.Any(t => t == "postal_code"))?
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
                viewModel = new Location
                {
                    CloseLocations = locations.OrderBy(c => c.Name).ToList(),
                    ShowLocation = true
                };
                return View("Locations", viewModel);
            }
        }

        [HttpGet("[action]/{locationStub}")]
        [HttpGet("[action]/{locationStub}/{featureStub}")]
        public IActionResult Locations(string locationStub, string featureStub)
        {
            Location[] locations = { };

            Feature[] features = { };

            LocationFeature[] locationFeatures = { };

            LocationGroup[] locationGroups = { };

            if (string.IsNullOrEmpty(locationStub))
            {
                return View("Locations", locations);
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                var locationViewModel = new LocationViewModel();

                foreach (var location in locations)
                {
                    if (location.Stub == locationStub)
                    {
                        locationViewModel.Location = location;
                        var featureList = new List<LocationsFeaturesViewModel>();
                        var neighbors = new List<Location>();
                        foreach (var item in locationFeatures)
                        {
                            if (item.LocationId == location.Id)
                            {
                                foreach (var feature in features)
                                {
                                    if (item.FeatureId == feature.Id)
                                    {
                                        var locationFeature = new LocationsFeaturesViewModel
                                        {
                                            Text = item.Text,
                                            Stub = feature.Stub,
                                            Name = feature.Name,
                                            ImagePath = feature.ImagePath,
                                            FontAwesome = feature.FontAwesome,
                                            BodyText = feature.BodyText,
                                            RedirectUrl = feature.Name == "Facebook"
                                                ? location.Facebook
                                                : item.RedirectUrl
                                        };
                                        featureList.Add(locationFeature);
                                    }
                                }
                            }
                        }
                        var groupId = locationGroups.FirstOrDefault(c => c.LocationId == location.Id && c.GroupId == 1);
                        if (groupId != null)
                        {
                            foreach (var group in locationGroups)
                            {
                                if (locations.First(_ => _.Id == group.LocationId).Id != location.Id && groupId.GroupId == group.GroupId)
                                {
                                    neighbors.Add(locations.First(_ => _.Id == group.LocationId));
                                }
                            }
                        }
                        locationViewModel.LocationFeatures = featureList;
                        locationViewModel.NearbyLocations = neighbors;
                        locationViewModel.NearbyCount = 1;
                        break;
                    }
                }
                return View("LocationDetails", locationViewModel);
            }
            else
            {
                var locationViewModel = new LocationViewModel();

                foreach (var location in locations)
                {
                    if (location.Stub == locationStub)
                    {
                        locationViewModel.Location = location;
                        var featureList = new List<LocationsFeaturesViewModel>();
                        foreach (var item in locationFeatures)
                        {
                            if (item.LocationId == location.Id)
                            {
                                foreach (var feature in features)
                                {
                                    if (feature.Stub == featureStub)
                                    {
                                        var locationFeature = new LocationsFeaturesViewModel
                                        {
                                            RedirectUrl = item.RedirectUrl,
                                            Text = item.Text,
                                            Stub = feature.Stub,
                                            Name = feature.Name,
                                            ImagePath = feature.ImagePath,
                                            FontAwesome = feature.FontAwesome,
                                            BodyText = feature.BodyText
                                        };
                                        featureList.Add(locationFeature);
                                    }
                                }
                                locationViewModel.LocationFeatures = featureList;
                                break;
                            }
                        }
                        break;
                    }
                }
                return View("LocationFeatureDetails", locationViewModel);
            }
        }

        [NonAction]
        public Location LookupLocation(double latitude, double longitude)
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
                location.MapLink = "https://maps.googleapis.com/maps/api/staticmap?center=" + latitude.ToString() + "," + longitude.ToString() + "&zoom=12&maptype=roadmap&format=png&visual_refresh=true";
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
