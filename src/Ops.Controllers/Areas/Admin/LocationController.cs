using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BranchLocator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.TagHelpers;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class LocationController : BaseController<LocationController>
    {
        private readonly ILocationService _locationService;
        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;

        public LocationController(IConfiguration config,
            ServiceFacades.Controller<LocationController> context,
            ILocationService locationService,
            ILogger<HomeController> logger) : base(context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index(string locationStub)
        {
            var locationList = await _locationService.GetAllLocationsAsync();
            var location = new Location();

            var viewModel = new LocationViewModel
            {
                Location = location,
                AllLocations = locationList
            };

            return View(viewModel);
        }
        [HttpGet("{locationStub}")]
        [HttpGet("AddLocation")]
        public async Task<IActionResult> Location(string locationStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                var location = new Location();

                var viewModel = new LocationViewModel
                {
                    Location = location,
                };

                return View("LocationDetails", viewModel);
            }
            else
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);

                var viewModel = new LocationViewModel
                {
                    Location = location,
                };
                try
                {
                    using (var client = new HttpClient())
                    {
                        var apikey = _config[Configuration.OpsAPIGoogleMaps];

                        GeocodeResult geoResult = null;
                        string stringResult = null;

                        try
                        {
                            var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={location.Address},+{location.City},+{location.State}&key={apikey}");
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
                            var lat = geoResult.Results?
                                .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                                .Geometry?
                                .Location?
                                .Lat;
                            var lng = geoResult.Results?
                                .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                                .Geometry?
                                .Location?
                                .Lng;
                            if (lat.HasValue && lng.HasValue)
                            {
                                location.Latitude = lat.Value;
                                location.GeoLocation = lng.ToString() + ", " + lat.ToString();
                            }

                            else
                            {
                                _logger.LogInformation($"Could not find latitude and longitude when geocoding {location.Address}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Problem looking up postal code for coordinates {location.Address}: {ex.Message}");
                }


                return View("LocationDetails", viewModel);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateLocation(Location location)
        {
            var success = false;
            var message = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    var apikey = _config[Configuration.OpsAPIGoogleMaps];

                    GeocodeResult geoResult = null;
                    string stringResult = null;

                    try
                    {
                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={location.Address},+{location.City},+{location.State}&key={apikey}");
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
                        var lat = geoResult.Results?
                            .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                            .Geometry?
                            .Location?
                            .Lat;
                        var lng = geoResult.Results?
                            .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                            .Geometry?
                            .Location?
                            .Lng;

                        if (lat.HasValue && lng.HasValue)
                        {
                            location.Latitude = lat.Value;
                            location.GeoLocation = lng.ToString() + ", " + lat.ToString();
                        }
                        else
                        {
                            _logger.LogInformation($"Could not find latitude and longitude when geocoding {location.Address}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Problem looking up postal code for coordinates {location.Address}: {ex.Message}");
            }


            try
            {
                await _locationService.AddAsync(location);
                ShowAlertSuccess($"Added Location: {location.Name}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }
            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteLocation(Location location)
        {
            var success = false;
            var message = string.Empty;

            try
            {
                await _locationService.DeleteAsync(location.Id);
                ShowAlertSuccess($"Deleted Location: {location.Name}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditLocation(Location location)
        {
            var success = false;
            var message = string.Empty;

            try
            {
                await _locationService.EditAsync(location);
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }
            return Json(new { success, message });
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
            try
            {
                using (var client = new HttpClient())
                {
                    GeocodePlace geoPlace = null;
                    string stringResult = null;
                    try
                    {
                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/textsearch/json?query={addrstr}&key={apikey}");
                        response.EnsureSuccessStatusCode();

                        stringResult = await response.Content.ReadAsStringAsync();

                        geoPlace = JsonConvert.DeserializeObject<GeocodePlace>(stringResult);
                        var results = new List<PlaceDetailsResult>();
                        foreach (var result in geoPlace.Results)
                        {
                            if (!string.IsNullOrEmpty(result.PlaceId))
                            {
                                GeocodePlaceDetails geoDetailPlace = new GeocodePlaceDetails();
                                string stringDetailResult = null;

                                var detailResponse = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/details/json?placeid={result.PlaceId}&key={apikey}");
                                detailResponse.EnsureSuccessStatusCode();

                                stringDetailResult = await detailResponse.Content.ReadAsStringAsync();
                                geoDetailPlace = JsonConvert.DeserializeObject<GeocodePlaceDetails>(stringDetailResult);
                                if (geoDetailPlace.Results.Count()>0)
                                {
                                    foreach (var detail in geoDetailPlace.Results)
                                    {
                                        results.Add(detail);
                                    }
                                }
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

                        _logger.LogError($"Error parsing Geocode API JSON: {ex.Message} - {stringResult}");
                        return Json(new
                        {
                            success
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Problem looking up postal code for coordinates {location.Address}: {ex.Message}");
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
                using (var client = new HttpClient())
                {
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
                        _logger.LogError($"Error parsing Geocode API JSON: {ex.Message} - {stringResult}");
                    }

                    if (geoResult?.Results?.Count() > 0)
                    {
                        var lat = geoResult.Results?
                            .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                            .Geometry?
                            .Location?
                            .Lat;
                        var lng = geoResult.Results?
                            .FirstOrDefault(_ => _.Types.Any(__ => __ == "premise"))?
                            .Geometry?
                            .Location?
                            .Lng;

                        if (lat.HasValue && lng.HasValue)
                        {
                            location.Latitude = lat.Value;
                            location.Longitude = lng.Value;
                            location.GeoLocation = lng.ToString() + ", " + lat.ToString();
                        }
                        else
                        {
                            _logger.LogInformation($"Could not find latitude and longitude when geocoding {location.Address}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Problem looking up postal code for coordinates {location.Address}: {ex.Message}");
            }
            return location;
        }

    }
}
