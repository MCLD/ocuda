using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]

    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{locationStub}")]
        [HttpGet("[action]/{locationStub}/{featureStub}")]
        public IActionResult Locations(string locationStub, string featureStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return View("Locations");
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                var locationViewModel = new LocationViewModel();
                var locationsFeat = new List<LocationFeature>();
                var locations = new List<Location>();
                var features = new List<Feature>();

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
                var locationsFeat = new List<LocationFeature>();
                var locations = new List<Location>();
                var features = new List<Feature>();

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
    }
}
