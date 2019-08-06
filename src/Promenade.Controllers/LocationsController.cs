using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BranchLocator.Helpers;
using BranchLocator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Locations;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly LocationService _locationService;

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            LocationService locationService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        [HttpGet("[action]/{zip}")]
        [HttpGet("[action]/{latitude}/{longitude}")]
        public async Task<IActionResult> Find(double latitude = 0, double longitude = 0, string zip = null)
        {

            if (!string.IsNullOrWhiteSpace(zip) && (latitude.Equals(0) && longitude.Equals(0)))
            {
                var viewModel = new LocationViewModel
                {
                    LocationSearchable = true
                };
                var location = new Location();

                using (var client = new HttpClient())
                {
                    try
                    {
                        var apikey = _config[Utility.Keys.Configuration.PromAPIGoogleMaps];

                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={zip}&key={apikey}");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        dynamic jsonResult = JsonConvert.DeserializeObject(stringResult);

                        if (jsonResult.results.Count > 0)
                        {
                            var result = jsonResult.results[0];
                            double newLat = result.geometry.location.lat;
                            double newLong = result.geometry.location.lng;

                            return View("Locations", await LookupLocationAsync(newLat, newLong));
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
                        viewModel.LocationSearchable = false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, ex.Message);
                        TempData["AlertDanger"] = "An error occured, please try again later.";
                    }
                }
                location.CloseLocations = (await _locationService.GetAllLocationsAsync()).OrderBy(c => c.Name).ToList();
                viewModel.Location = location;
                return View("Locations", viewModel);
            }
            else if (!latitude.Equals(0) && !longitude.Equals(0) && string.IsNullOrEmpty(zip))
            {
                var viewModel = await LookupLocationAsync(latitude, longitude);
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
                var location = new Location
                {
                    CloseLocations = (await _locationService.GetAllLocationsAsync()).OrderBy(c => c.Name).ToList()
                };
                var viewModel = new LocationViewModel
                {
                    Location = location,
                    LocationSearchable = false
                };
                foreach (var item in viewModel.Location.CloseLocations)
                {
                    item.WeeklyHours = await _locationService.GetFormattedWeeklyHoursAsync(item.Id);
                }
                return View("Locations", viewModel);
            }
        }

        [HttpGet("{locationStub}")]
        [HttpGet("{locationStub}/{featureStub}")]
        public async Task<IActionResult> Locations(string locationStub, string featureStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return await Find();
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                var locationViewModel = new LocationViewModel
                {
                    LocationFeatures = new List<LocationsFeaturesViewModel>(),
                    Location = await _locationService.GetLocationByStubAsync(locationStub)
                };
                locationViewModel.Location.LocationHours = await _locationService.GetFormattedWeeklyHoursAsync(locationViewModel.Location.Id);
                var features = await _locationService.GetLocationsFeaturesAsync(locationStub);

                foreach (var feature in features.OrderBy(_ => _.Name).ToList())
                {
                    var locationFeature = await _locationService.GetLocationFeatureByIds(locationViewModel.Location.Id, feature.Id);
                    var locationfeatureModel = new LocationsFeaturesViewModel
                    {
                        BodyText = CommonMark.CommonMarkConverter.Convert(feature.BodyText),
                        FontAwesome = feature.FontAwesome,
                        ImagePath = feature.ImagePath,
                        Name = feature.Name,
                        RedirectUrl = locationFeature.RedirectUrl,
                        Stub = feature.Stub,
                        Text = CommonMark.CommonMarkConverter.Convert(locationFeature.Text)
                    };
                    if (!feature.FontAwesome.Contains("fa-inverse"))
                    {
                        locationfeatureModel.InnerSpan = "<strong>7</strong>";
                    }
                    locationViewModel.LocationFeatures.Add(locationfeatureModel);
                }
                var neighbors = await _locationService.GetLocationsNeighborsAsync(locationStub);
                locationViewModel.LocationNeighborGroup = await _locationService.GetLocationsNeighborGroup(locationStub);
                if (neighbors.Any())
                {
                    locationViewModel.NearbyLocations = neighbors;
                    locationViewModel.NearbyCount = locationViewModel.NearbyLocations.Count;
                }
                else
                {
                    locationViewModel.NearbyCount = 0;
                }

                return View("LocationDetails", locationViewModel);
            }
            else
            {
                var locationViewModel = new LocationViewModel();
                var locationFeatureViewModel = new List<LocationsFeaturesViewModel>();

                locationViewModel.Location = await _locationService.GetLocationByStubAsync(locationStub);
                var feature = (await _locationService.GetLocationsFeaturesAsync(locationStub)).SingleOrDefault(_ => _.Stub == featureStub);
                if (feature != null)
                {
                    var locationFeature = await _locationService.GetLocationFeatureByIds(locationViewModel.Location.Id, feature.Id);

                    var locationfeatureModel = new LocationsFeaturesViewModel
                    {
                        BodyText = CommonMark.CommonMarkConverter.Convert(feature.BodyText),
                        FontAwesome = feature.FontAwesome,
                        ImagePath = feature.ImagePath,
                        Name = feature.Name,
                        RedirectUrl = locationFeature.RedirectUrl,
                        Stub = feature.Stub,
                        Text = CommonMark.CommonMarkConverter.Convert(locationFeature.Text)
                    };

                    locationFeatureViewModel.Add(locationfeatureModel);
                    locationViewModel.LocationFeatures = locationFeatureViewModel;

                    return View("LocationFeatureDetails", locationViewModel);
                }
                else
                {
                    return await Locations(locationStub, "");
                }
            }
        }

        [NonAction]
        public async Task<Location> LookupLocationAsync(double latitude, double longitude)
        {
            var viewModel = new Location();
            var locations = (await _locationService.GetAllLocationsAsync()).OrderBy(c => c.Name).ToList();

            foreach (var location in locations)
            {
                var geolocation = location.GeoLocation
                    .Split(',')
                    .Select(_ => Convert.ToDouble(_)).ToList();
                location.Distance = HaversineHelper
                    .Calculate(geolocation[1], geolocation[0], latitude, longitude);
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
