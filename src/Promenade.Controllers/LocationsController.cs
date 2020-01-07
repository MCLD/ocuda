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
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly LocationService _locationService;

        public static string Name { get { return "Locations"; } }

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            LocationService locationService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        [HttpGet("[action]/{Zip}")]
        [HttpGet("[action]/{latitude}/{longitude}")]
        public async Task<IActionResult> Find(double? latitude = null, double? longitude = null,
            string zip = null)
        {
            var apiKey = _config[Utility.Keys.Configuration.PromAPIGoogleMaps];

            var viewModel = new LocationViewModel
            {
                CanSearchAddress = !string.IsNullOrWhiteSpace(apiKey),
                Zip = zip?.Trim()
            };

            var searchLatitude = latitude;
            var searchLongitude = longitude;

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                if (!string.IsNullOrWhiteSpace(zip))
                {
                    using var client = new HttpClient();
                    try
                    {
                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={zip}&key={apiKey}");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        dynamic jsonResult = JsonConvert.DeserializeObject(stringResult);

                        if (jsonResult.results.Count > 0)
                        {
                            var result = jsonResult.results[0];
                            searchLatitude = result.geometry.location.lat;
                            searchLongitude = result.geometry.location.lng;
                        }
                        else
                        {
                            _logger.LogError("No geocoding results for {ZIPCode}", zip);
                            TempData["AlertDanger"] = $"Unable to locate ZIP Code: <strong>{zip}</strong>.";
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogCritical(ex,
                            "Google API error geocoding {ZIPCode}: {Message}",
                            viewModel.Zip,
                            ex.Message);
                        TempData["AlertDanger"] = "An error occured, please try again later.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical("Error geocoding {ZIPCode}: {Message}",
                            viewModel.Zip,
                            ex.Message);
                        TempData["AlertDanger"] = "An error occured, please try again later.";
                    }
                }
                else if (latitude.HasValue && longitude.HasValue)
                {
                    // try to get the zip code to display to the user
                    var latlng = $"{latitude},{longitude}";
                    try
                    {
                        using var client = new HttpClient();
                        GeocodeResult geoResult = null;
                        string stringResult = null;

                        try
                        {
                            var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?latlng={latlng}&key={apiKey}");
                            response.EnsureSuccessStatusCode();

                            stringResult = await response.Content.ReadAsStringAsync();

                            geoResult = JsonConvert.DeserializeObject<GeocodeResult>(stringResult);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error parsing Geocode API JSON: {Message} - {Result}",
                                ex.Message,
                                stringResult);
                        }

                        if (geoResult?.Results?.Count() > 0)
                        {
                            viewModel.Zip = geoResult?
                                .Results?
                                .FirstOrDefault(_ => _.Types.Any(__ => __ == "postal_code"))?
                                .AddressComponents?
                                .FirstOrDefault()?
                                .ShortName;
                            if (string.IsNullOrEmpty(viewModel.Zip))
                            {
                                _logger.LogWarning("Could not find postal code when reverse geocoding {Coordinates}",
                                    latlng);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Problem looking up postal code for coordinates {Coordinates}: {Message}",
                            latlng,
                            ex.Message);
                    }
                    return View("Locations", viewModel);
                }
            }

            viewModel.Locations = await _locationService.GetAllLocationsAsync();

            foreach (var location in viewModel.Locations)
            {
                location.CurrentStatus = await _locationService.GetCurrentStatusAsync(location.Id);
            }

            if (searchLatitude.HasValue && searchLongitude.HasValue)
            {
                foreach (var location in viewModel.Locations)
                {
                    var geolocation = location.GeoLocation
                        .Split(',')
                        .Select(_ => Convert.ToDouble(_)).ToList();
                    location.Distance = HaversineHelper.Calculate(geolocation[0], geolocation[1],
                        searchLatitude.Value, searchLongitude.Value);
                }

                viewModel.Locations = viewModel.Locations.OrderBy(_ => _.Distance)
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
        [HttpGet("{locationStub}/{featureStub}")]
        public async Task<IActionResult> Locations(string locationStub, string featureStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return await Find();
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                var viewModel = new LocationDetailViewModel
                {
                    LocationFeatures = new List<LocationsFeaturesViewModel>(),
                    Location = await _locationService.GetLocationByStubAsync(locationStub)
                };
                viewModel.Location.Description = CommonMark.CommonMarkConverter.Convert(viewModel.Location.Description);
                viewModel.Location.PostFeatureDescription = CommonMark.CommonMarkConverter.Convert(viewModel.Location.PostFeatureDescription);
                viewModel.Location.LocationHours = await _locationService.GetFormattedWeeklyHoursAsync(viewModel.Location.Id);
                if (viewModel.Location.LocationHours != null)
                {
                    viewModel.StructuredLocationHours = (await _locationService.GetFormattedWeeklyHoursAsync(viewModel.Location.Id, true))
                        .Select(_ => $"{_.Days} {_.Time}").ToList();
                }
                var features = await _locationService.GetLocationsFeaturesAsync(locationStub);

                foreach (var feature in features.OrderBy(_ => _.Name).ToList())
                {
                    var locationFeature = await _locationService.GetLocationFeatureByIds(viewModel.Location.Id, feature.Id);
                    var locationfeatureModel = new LocationsFeaturesViewModel
                    {
                        BodyText = CommonMark.CommonMarkConverter.Convert(feature.BodyText),
                        Icon = feature.Icon,
                        ImagePath = feature.ImagePath,
                        Name = feature.Name,
                        RedirectUrl = locationFeature.RedirectUrl,
                        Stub = feature.Stub,
                        Text = CommonMark.CommonMarkConverter.Convert(locationFeature.Text)
                    };
                    if (!feature.Icon.Contains("fa-inverse"))
                    {
                        locationfeatureModel.InnerSpan = "<strong>7</strong>";
                    }
                    viewModel.LocationFeatures.Add(locationfeatureModel);
                }

                if (viewModel.Location.DisplayGroupId.HasValue)
                {
                    viewModel.LocationNeighborGroup = await _locationService
                        .GetLocationsNeighborGroup(viewModel.Location.DisplayGroupId.Value);

                    var neighbors = await _locationService
                        .GetLocationsNeighborsAsync(viewModel.Location.DisplayGroupId.Value);
                    if (neighbors.Count > 0)
                    {
                        viewModel.NearbyLocations = neighbors;
                        viewModel.NearbyCount = viewModel.NearbyLocations.Count;
                    }
                    else
                    {
                        viewModel.NearbyCount = 0;
                    }
                }

                PageTitle = viewModel.Location.Name;

                return View("LocationDetails", viewModel);
            }
            else
            {
                var viewModel = new LocationDetailViewModel();
                var locationFeatureViewModel = new List<LocationsFeaturesViewModel>();

                viewModel.Location = await _locationService.GetLocationByStubAsync(locationStub);
                var feature = (await _locationService.GetLocationsFeaturesAsync(locationStub)).SingleOrDefault(_ => _.Stub == featureStub);
                if (feature != null)
                {
                    var locationFeature = await _locationService.GetLocationFeatureByIds(viewModel.Location.Id, feature.Id);

                    var locationfeatureModel = new LocationsFeaturesViewModel
                    {
                        BodyText = CommonMark.CommonMarkConverter.Convert(feature.BodyText),
                        Icon = feature.Icon,
                        ImagePath = feature.ImagePath,
                        Name = feature.Name,
                        RedirectUrl = locationFeature.RedirectUrl,
                        Stub = feature.Stub,
                        Text = CommonMark.CommonMarkConverter.Convert(locationFeature.Text)
                    };

                    locationFeatureViewModel.Add(locationfeatureModel);
                    viewModel.LocationFeatures = locationFeatureViewModel;

                    PageTitle = feature.Name;

                    return View("LocationFeatureDetails", viewModel);
                }
                else
                {
                    return await Locations(locationStub, "");
                }
            }
        }
    }
}
