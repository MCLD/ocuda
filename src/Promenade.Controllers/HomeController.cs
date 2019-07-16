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
            var aguila = new Location { GeoLocation = "33.942968999999984,-113.173406", Longitude = -113.172069, Latitude = 33.942349, Id = 0, Name = "Aguila", Code = "AG", Stub = "aguila", MapLink = "https://goo.gl/maps/FHADwznVmfR2", Address = "51300 US-60", City = "Aguila", Zip = "85320", Description = "The Aguila Library opened in its new building in 2006. The 2,500 sq. ft. facility brings the latest in quality library services to this rural community. It contains a large Spanish materials collection for this farming community.", Facebook = "mcldag", SubscriptionLinkId = 0, EventLinkId = "0" };
            var centralExp = new Location { GeoLocation = "33.47780600000001,-112.07406200000003", Longitude = -112.074258, Latitude = 33.478004, Id = 1, Name = "Central Express", Code = "AD", Stub = "centralexpress", MapLink = "https://goo.gl/maps/DbFro1FfiBP2", Address = "2700 N Central Ave Ste 700", City = "Phoenix", Zip = "85004", Description = "The Central Express library is a pick up/drop off (i.e., no collection to browse) location for County Library materials. It's located in midtown Phoenix near Central Ave and Thomas Rd, a convenient stop for people who live, work, or pass through the area to pick up and drop off materials from 8:00am &ndash; 5:00pm, Monday &ndash; Friday. The Central Express library also serves as MCLD's administrative headquarters. This location offers onsite use of iPads and digital services, as well as new library card applications and renewals.", Facebook = "mcldaz", SubscriptionLinkId = null, EventLinkId = "ALL" };
            var edRobson = new Location { GeoLocation = "33.21841400000001,-111.88145799999995", Longitude = -111.881663, Latitude = 33.218343, Id = 2, Name = "Ed Robson", Code = "RO", Stub = "edrobson", MapLink = "https://goo.gl/maps/amHZCLcsT7T2", Address = "9330 E Riggs Rd", City = "Sun Lakes", Zip = "85248", Description = "The Ed Robson Library moved into its 10,000-sq. ft. facility in July 2004. This library serves Sun Lakes and surrounding communities.", Facebook = "mcldro", SubscriptionLinkId = 1, EventLinkId = "1" };
            var elMirage = new Location { GeoLocation = "33.61118299999999,-112.32589999999993", Longitude = -112.325751, Latitude = 33.611136, Id = 3, Name = "El Mirage", Code = "EM", Stub = "elmirage", MapLink = "https://goo.gl/maps/gQqWhaBHbAn", Address = "14011 N 1st Ave", City = "El Mirage", Zip = "85335", Description = "The El Mirage Library is housed in a 2500-sq. ft. building. The El Mirage Library serves a city comprised of a culturally deep-rooted Hispanic/Spanish population, as well as a rapidly increasing population of new families from nearby developments. The library has a collection designed to meet the needs of this diverse community and includes a large collection of Spanish materials. The library also has Internet accessible computers and regular bilingual programming activities.", Facebook = "mcldem", SubscriptionLinkId = 2, EventLinkId = "2" };
            Location[] locations = {edRobson, aguila, centralExp, elMirage };

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
            var aguila = new Location { Id = 0, Name = "Aguila", Code = "AG", Stub = "aguila", MapLink = "https://goo.gl/maps/FHADwznVmfR2", Address = "51300 US-60", City = "Aguila", Zip = "85320", Description = "The Aguila Library opened in its new building in 2006. The 2,500 sq. ft. facility brings the latest in quality library services to this rural community. It contains a large Spanish materials collection for this farming community.", Facebook = "mcldag", SubscriptionLinkId = 0, EventLinkId = "0" };
            var centralExp = new Location { Id = 1, Name = "Central Express", Code = "AD", Stub = "centralexpress", MapLink = "https://goo.gl/maps/DbFro1FfiBP2", Address = "2700 N Central Ave Ste 700", City = "Phoenix", Zip = "85004", Description = "The Central Express library is a pick up/drop off (i.e., no collection to browse) location for County Library materials. It's located in midtown Phoenix near Central Ave and Thomas Rd, a convenient stop for people who live, work, or pass through the area to pick up and drop off materials from 8:00am &ndash; 5:00pm, Monday &ndash; Friday. The Central Express library also serves as MCLD's administrative headquarters. This location offers onsite use of iPads and digital services, as well as new library card applications and renewals.", Facebook = "mcldaz", SubscriptionLinkId = null, EventLinkId = "ALL" };
            var edRobson = new Location { Id = 2, Name = "Ed Robson", Code = "RO", Stub = "edrobson", MapLink = "https://goo.gl/maps/amHZCLcsT7T2", Address = "9330 E Riggs Rd", City = "Sun Lakes", Zip = "85248", Description = "The Ed Robson Library moved into its 10,000-sq. ft. facility in July 2004. This library serves Sun Lakes and surrounding communities.", Facebook = "mcldro", SubscriptionLinkId = 1, EventLinkId = "1" };
            var elMirage = new Location { Id = 3, Name = "El Mirage", Code = "EM", Stub = "elmirage", MapLink = "https://goo.gl/maps/gQqWhaBHbAn", Address = "14011 N 1st Ave", City = "El Mirage", Zip = "85335", Description = "The El Mirage Library is housed in a 2500-sq. ft. building. The El Mirage Library serves a city comprised of a culturally deep-rooted Hispanic/Spanish population, as well as a rapidly increasing population of new families from nearby developments. The library has a collection designed to meet the needs of this diverse community and includes a large collection of Spanish materials. The library also has Internet accessible computers and regular bilingual programming activities.", Facebook = "mcldem", SubscriptionLinkId = 2, EventLinkId = "2" };
            Location[] locations = { aguila, centralExp, edRobson, elMirage };

            var volunteer = new Feature { Id = 0, Name = "Volunteer", FontAwesome = "fa fa-inverse fa-info-circle fa-stack-1x", ImagePath = "voluntold.jpg", Stub = "Volunteer", BodyText = "" };
            var sevenDay = new Feature { Id = 1, Name = "7-Day Express", FontAwesome = "fa-stack-1x", ImagePath = "day.jpg", Stub = "7Day", BodyText = "Sometimes called “Lucky Day” materials, we reserve a few copies of these popular books that can only be checked out from the branch. So if you find one, it’s your lucky day! Here’s what you need to know:" };
            var citizenSci = new Feature { Id = 2, Name = "Citizen Science", FontAwesome = "fa fa-inverse fa-flask fa-stack-1x", ImagePath = "citizen.jpg", Stub = "CitizenScience", BodyText = "Citizen Science kits empower our customers to participate in research with instruments checked out from their local library. See which kits are available from the Ed Robson Library. Watch videos about the Zombee Hunting and Bio Diversity kits to learn more." };
            var facebook = new Feature { Id = 3, Name = "Facebook", FontAwesome = "fab fa-inverse fa-facebook-f fa-stack-1x", ImagePath = "facebook.jpg", Stub = "", BodyText = "" };
            Feature[] features = { volunteer, sevenDay, citizenSci, facebook };

            var aguilaVolunteer = new LocationFeature { LocationId = 0, FeatureId = 0, RedirectUrl = "https://mcldaz.org/volunteer/AG" };
            var edVolunteer = new LocationFeature { LocationId = 2, FeatureId = 0, RedirectUrl = "https://mcldaz.org/volunteer/RO" };
            var elVolunteer = new LocationFeature { LocationId = 3, FeatureId = 0, RedirectUrl = "https://mcldaz.org/volunteer/EM" };

            var aguilaSeven = new LocationFeature { LocationId = 0, FeatureId = 1, Text = " - <span class=\"fa - stack - 1x\" style=\"color: white; \"><strong>7</strong></span> 7-day checkout" };
            var edSeven = new LocationFeature { LocationId = 2, FeatureId = 1, Text = " - <span class=\"fa - stack - 1x\" style=\"color: white; \"><strong>7</strong></span> 7-day checkout" };
            var elSeven = new LocationFeature { LocationId = 3, FeatureId = 1, Text = " - <span class=\"fa - stack - 1x\" style=\"color: white; \"><strong>7</strong></span> 7-day checkout" };

            var edCitizen = new LocationFeature { LocationId = 2, FeatureId = 2, Text = "See which kits are available from the Ed Robson Library." };

            var aguilaFace = new LocationFeature { LocationId = 0, FeatureId = 3, RedirectUrl = "" };
            var centralFace = new LocationFeature { LocationId = 1, FeatureId = 3 };
            var edFace = new LocationFeature { LocationId = 2, FeatureId = 3 };
            var elFace = new LocationFeature { LocationId = 3, FeatureId = 3 };

            LocationFeature[] locationsFeat = { aguilaVolunteer, aguilaSeven, aguilaFace, edCitizen, edFace, edSeven, edVolunteer, elVolunteer, elSeven, elFace, centralFace };

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
                                locationViewModel.LocationFeatures = featlist;
                            }
                        }
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
            var aguila = new Location { GeoLocation = "33.942968999999984,-113.173406", Longitude = -113.172069, Latitude = 33.942349, Id = 0, Name = "Aguila", Code = "AG", Stub = "aguila", MapLink = "https://goo.gl/maps/FHADwznVmfR2", Address = "51300 US-60", City = "Aguila", Zip = "85320", Description = "The Aguila Library opened in its new building in 2006. The 2,500 sq. ft. facility brings the latest in quality library services to this rural community. It contains a large Spanish materials collection for this farming community.", Facebook = "mcldag", SubscriptionLinkId = 0, EventLinkId = "0" };
            var centralExp = new Location { GeoLocation = "33.47780600000001,-112.07406200000003", Longitude = -112.074258, Latitude = 33.478004, Id = 1, Name = "Central Express", Code = "AD", Stub = "centralexpress", MapLink = "https://goo.gl/maps/DbFro1FfiBP2", Address = "2700 N Central Ave Ste 700", City = "Phoenix", Zip = "85004", Description = "The Central Express library is a pick up/drop off (i.e., no collection to browse) location for County Library materials. It's located in midtown Phoenix near Central Ave and Thomas Rd, a convenient stop for people who live, work, or pass through the area to pick up and drop off materials from 8:00am &ndash; 5:00pm, Monday &ndash; Friday. The Central Express library also serves as MCLD's administrative headquarters. This location offers onsite use of iPads and digital services, as well as new library card applications and renewals.", Facebook = "mcldaz", SubscriptionLinkId = null, EventLinkId = "ALL" };
            var edRobson = new Location { GeoLocation = "33.21841400000001,-111.88145799999995", Longitude = -111.881663, Latitude = 33.218343, Id = 2, Name = "Ed Robson", Code = "RO", Stub = "edrobson", MapLink = "https://goo.gl/maps/amHZCLcsT7T2", Address = "9330 E Riggs Rd", City = "Sun Lakes", Zip = "85248", Description = "The Ed Robson Library moved into its 10,000-sq. ft. facility in July 2004. This library serves Sun Lakes and surrounding communities.", Facebook = "mcldro", SubscriptionLinkId = 1, EventLinkId = "1" };
            var elMirage = new Location { GeoLocation = "33.61118299999999,-112.32589999999993", Longitude = -112.325751, Latitude = 33.611136, Id = 3, Name = "El Mirage", Code = "EM", Stub = "elmirage", MapLink = "https://goo.gl/maps/gQqWhaBHbAn", Address = "14011 N 1st Ave", City = "El Mirage", Zip = "85335", Description = "The El Mirage Library is housed in a 2500-sq. ft. building. The El Mirage Library serves a city comprised of a culturally deep-rooted Hispanic/Spanish population, as well as a rapidly increasing population of new families from nearby developments. The library has a collection designed to meet the needs of this diverse community and includes a large collection of Spanish materials. The library also has Internet accessible computers and regular bilingual programming activities.", Facebook = "mcldem", SubscriptionLinkId = 2, EventLinkId = "2" };
            Location[] locations = { aguila, centralExp, edRobson, elMirage };

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
