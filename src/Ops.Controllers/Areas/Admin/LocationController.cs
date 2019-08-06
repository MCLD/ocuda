using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BranchLocator.Models;
using BranchLocator.Models.PlaceDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
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

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new LocationViewModel
            {
                AllLocations = locationList.Data,
                PaginateModel = paginateModel
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
                location.IsNewLocation = true;
                var viewModel = new LocationViewModel
                {
                    Location = location,
                };

                return View("LocationDetails", viewModel);
            }
            else
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                location.IsNewLocation = false;
                var viewModel = new LocationViewModel
                {
                    Location = location,
                };
                try
                {
                    location = await GetLatLng(location);
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
                _logger.LogError(ex, $"Problem looking up postal code for coordinates {location.Address}: {ex.Message}");
                ShowAlertDanger($"Unable to find Location's address: {location.Address}");
                location.IsNewLocation = true;
                var viewModel = new LocationViewModel
                {
                    Location = location,
                };

                return View("LocationDetails", viewModel);
            }


            try
            {
                await _locationService.AddLocationAsync(location);
                ShowAlertSuccess($"Added Location: {location.Name}");
                location.IsNewLocation = true;
                return RedirectToAction("Location", new { locationStub = location.Stub});
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Create Location: {ex.Message}");
                location.IsNewLocation = true;
                var viewModel = new LocationViewModel
                {
                    Location = location,
                };

                return View("LocationDetails", viewModel);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteLocation(Location location)
        {
            try
            {
                await _locationService.DeleteAsync(location.Id);
                ShowAlertSuccess($"Deleted Location: {location.Name}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Delete Location {location.Name}: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditLocation(Location location)
        {
            try
            {
                await _locationService.EditAsync(location);
                ShowAlertSuccess($"Updated Location: {location.Name}");
                location.IsNewLocation = false;
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Update Location: {location.Name}");
                location.IsNewLocation = false;
                var viewModel = new LocationViewModel
                {
                    Location = location,
                };

                return View("LocationDetails", viewModel);
            }
            return RedirectToAction("Location", new { locationStub = location.Stub});
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
                using (var client = new HttpClient())
                {
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
                            if(geoDetailPlace != null)
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
