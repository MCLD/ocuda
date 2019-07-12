using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[controller]")]
    public class LocationsController : BaseController
    {
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(ILogger<LocationsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Index), "Home");
        }

        [Route("{stub}")]
        public IActionResult Branches(string stub)
        {
            if (stub == "")
            {
                return RedirectToAction(nameof(Index), "Home");
            }
            var model = new LocationsViewModel();
            var locationsFeat = new List<LocationsFeatures>();
            var locations = new List<Locations>();
            var features = new List<Features>();

            foreach (var location in locations)
            {
                if (location.Stub == stub)
                {
                    model.Location = location;
                    var featlist = new List<LocationsFeaturesViewModel>();
                    foreach (var item in locationsFeat)
                    {
                        if (item.LocationId == location.Id)
                        {
                            foreach (var feat in features)
                            {
                                if (item.FeatureId == feat.Id)
                                {
                                    var locafeat = new LocationsFeaturesViewModel();
                                    if (feat.Name == "Facebook")
                                    {
                                        if (stub != "hollyhock")
                                        {
                                            locafeat.RedirectUrl = "http://facebook.com/" + location.Facebook;
                                        }
                                    }
                                    else
                                    {
                                        locafeat.RedirectUrl = item.RedirectUrl;
                                    }
                                    locafeat.Text = item.Text;
                                    locafeat.Stub = feat.Stub;
                                    locafeat.Name = feat.Name;
                                    locafeat.ImagePath = feat.ImagePath;
                                    locafeat.FontAwesome = feat.FontAwesome;
                                    locafeat.BodyText = feat.BodyText;
                                    featlist.Add(locafeat);
                                }
                            }
                            model.LocationFeatures = featlist;
                        }
                    }
                    break;
                }
            }
            return View("Locations", model);
        }

        [Route("{locaStub}/{featStub}")]
        public IActionResult Features(string locaStub, string featStub)
        {
            var model = new LocationsViewModel();
            var locationsFeat = new List<LocationsFeatures>();
            var locations = new List<Locations>();
            var features = new List<Features>();

            foreach (var location in locations)
            {
                if (location.Stub == locaStub)
                {
                    model.Location = location;
                    var featlist = new List<LocationsFeaturesViewModel>();
                    foreach (var item in locationsFeat)
                    {
                        if (item.LocationId == location.Id)
                        {
                            foreach (var feat in features)
                            {
                                if (feat.Stub == featStub)
                                {
                                    var locafeat = new LocationsFeaturesViewModel();
                                    locafeat.RedirectUrl = item.RedirectUrl;
                                    locafeat.Text = item.Text;
                                    locafeat.Stub = feat.Stub;
                                    locafeat.Name = feat.Name;
                                    locafeat.ImagePath = feat.ImagePath;
                                    locafeat.FontAwesome = feat.FontAwesome;
                                    locafeat.BodyText = feat.BodyText;
                                    featlist.Add(locafeat);
                                }
                            }
                            model.LocationFeatures = featlist;
                            break;
                        }
                    }
                    break;
                }
            }
            return View("LocationsFeatures",model);
        }
    }
}
