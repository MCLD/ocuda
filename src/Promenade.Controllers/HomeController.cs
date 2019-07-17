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
        [HttpGet("[action]/{address}")]
        [HttpGet("[action]/{address}/{mobile}")]
        public async Task<IActionResult> LocateZip(string address = null, int? mobile = null)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        var apikey = _config["APIKey"];

                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={apikey}");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        dynamic jsonResult = JsonConvert.DeserializeObject(stringResult);

                        if (jsonResult.results.Count > 0)
                        {
                            var result = jsonResult.results[0];
                            double latitude = result.geometry.location.lat;
                            double longitude = result.geometry.location.lng;

                            Location viewModel = LibraryLookup(latitude, longitude);
                            viewModel.FormattedAddress = result.formatted_address;

                            return View("Locations",viewModel);
                        }
                        else
                        {
                            _logger.LogError($"No geocoding results for address \"{address}\"");
                            TempData["AlertDanger"] = $"Unable to locate address <strong>\"{address}\"</strong>.";
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
            }
            if (mobile == 1)
            {
                return View(new Location { ShowLocation = true });
            }
            return View("Locations");
        }

        [HttpGet("Locations")]
        [HttpGet("Locations/[action]")]
        public async Task<IActionResult> Find(double latitude = 0, double longitude = 0, string address = null, int? mobile = null)
        {
            Location[] locations = {};

            if((latitude!=0 && longitude!=0) && address==null)
            {
                var viewModel = LibraryLookup(latitude, longitude);
                viewModel.ShowLocation = true;
                var latlng = $"{latitude},{longitude}";

                // try to get the zip code to display to the user
                try
                {
                    using (var client = new HttpClient())
                    {
                        var apikey = _config["APIKey"];

                        var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?latlng={latlng}&key={apikey}");
                        response.EnsureSuccessStatusCode();

                        GeocodeResult geoResult = null;
                        var stringResult = await response.Content.ReadAsStringAsync();

                        try
                        {
                            geoResult = JsonConvert.DeserializeObject<GeocodeResult>(stringResult);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error parsing Geocode API JSON: {ex.Message} - {stringResult}");
                        }

                        if (geoResult?.Results?.Count() > 0)
                        {
                            viewModel.Address = geoResult.Results?
                                .FirstOrDefault()?
                                .AddressComponents?
                                .Where(_ => _.Types.Any(t => t == "postal_code"))
                                .FirstOrDefault()?
                                .ShortName;
                            if (viewModel.Address == null)
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
                if (mobile == 1)
                {
                    viewModel.ShowLocation = true;
                }
                return View("Locations", viewModel);
            }
            else if((latitude==0 && longitude==0) && address != null)
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    using (var client = new HttpClient())
                    {
                        try
                        {
                            var apikey = _config["APIKey"];

                            var response = await client.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={apikey}");
                            response.EnsureSuccessStatusCode();

                            var stringResult = await response.Content.ReadAsStringAsync();
                            dynamic jsonResult = JsonConvert.DeserializeObject(stringResult);

                            if (jsonResult.results.Count > 0)
                            {
                                var result = jsonResult.results[0];
                                latitude = result.geometry.location.lat;
                                longitude = result.geometry.location.lng;

                                Location viewModel = LibraryLookup(latitude, longitude);
                                viewModel.FormattedAddress = result.formatted_address;
                                if (mobile == 1)
                                {
                                    viewModel.ShowLocation = true;
                                }
                                return View("Locations", viewModel);
                            }
                            else
                            {
                                _logger.LogError($"No geocoding results for address \"{address}\"");
                                TempData["AlertDanger"] = $"Unable to locate address <strong>\"{address}\"</strong>.";
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
                }
                return View("Locations");
            }
            else
            {
                var viewModel = new Location();
                viewModel.CloseLocations = locations.OrderBy( c => c.Name).ToList();
                if (mobile==1)
                {
                    viewModel.ShowLocation = true;
                }
                return View("Locations", viewModel);
            }
        }

        [HttpGet("[action]/{locationStub}")]
        [HttpGet("[action]/{locationStub}/{featureStub}")]
        public IActionResult Locations(string locationStub, string featureStub)
        {
            Location[] locations = {  };


            Feature[] features = {  };


            LocationFeature[] locationsFeat = {  };

            var group = new Group { Id = 1, GroupType="distance" };


            LocationGroup[] locagroups = { };


            if (string.IsNullOrEmpty(locationStub))
            {

                return View("Locations",locations);
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                var locationViewModel = new LocationViewModel();

                foreach (var location in locations)
                {
                    if (location.Stub == locationStub)
                    {
                        locationViewModel.Location = location;
                        var featlist = new List<LocationsFeaturesViewModel>();
                        var neighbors = new List<Location>();
                        foreach (var item in locationsFeat)
                        {
                            if (item.LocationId == location.Id)
                            {
                                foreach (var feat in features)
                                {
                                    if (item.FeatureId == feat.Id)
                                    {
                                        var locafeat = new LocationsFeaturesViewModel
                                        {
                                            Text = item.Text,
                                            Stub = feat.Stub,
                                            Name = feat.Name,
                                            ImagePath = feat.ImagePath,
                                            FontAwesome = feat.FontAwesome,
                                            BodyText = feat.BodyText,
                                            RedirectUrl = feat.Name == "Facebook"
                                            ? location.Facebook
                                            : item.RedirectUrl
                                        };
                                        featlist.Add(locafeat);
                                    }
                                }
                            }
                        }
                        var groupid = locagroups.First(c => c.LocationId == location.Id && c.GroupId == 1).GroupId;
                        foreach (var groupy in locagroups)
                        {
                            if (locations.First(c => c.Id == groupy.LocationId).Id != location.Id)
                            {
                                if (groupid == groupy.GroupId)
                                {
                                    var obj = locations.First(c => c.Id == groupy.LocationId);
                                    neighbors.Add(obj);
                                }
                            }
                        }
                        locationViewModel.LocationFeatures = featlist;
                        locationViewModel.NearByLocations = neighbors;
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
                        var featlist = new List<LocationsFeaturesViewModel>();
                        foreach (var item in locationsFeat)
                        {
                            if (item.LocationId == location.Id)
                            {
                                foreach (var feat in features)
                                {
                                    if (feat.Stub == featureStub)
                                    {
                                        var locafeat = new LocationsFeaturesViewModel
                                        {
                                            RedirectUrl = item.RedirectUrl,
                                            Text = item.Text,
                                            Stub = feat.Stub,
                                            Name = feat.Name,
                                            ImagePath = feat.ImagePath,
                                            FontAwesome = feat.FontAwesome,
                                            BodyText = feat.BodyText

                                        };
                                        featlist.Add(locafeat);
                                    }
                                }
                                locationViewModel.LocationFeatures = featlist;
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
        public Location LibraryLookup(double latitude, double longitude)
        {
            var viewModel = new Location();
            Location[] locations = {};

            List<Location> model = null;
            foreach (var location in locations)
            {
                var geolocation = location.GeoLocation
                    .Split(',')
                    .Select(_ => Convert.ToDouble(_)).ToList();
                location.Distance = HaversineHelper
                    .Calculate(geolocation[0], geolocation[1], latitude, longitude);
                location.MapLink = "https://maps.googleapis.com/maps/api/staticmap?center="+latitude.ToString() + "," + longitude.ToString()+ "&zoom=12&maptype=roadmap&format=png&visual_refresh=true";
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
