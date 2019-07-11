using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[controller]")]
    class LocationsController : BaseController
    {
        private readonly ILogger<LocationsController> _logger;
        public LocationsController(ILogger<LocationsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("")]
        [Route("[action]/{stub}")]
        public IActionResult Index(string stub)
        {
            //if(stub == "")
            //{
            //    return RedirectToAction(nameof(Index), "Home");
            //}

            var aguila = new Locations {Id=0, Name = "Aguila", BranchCode="AG",Stub = "aguila", MapLink= "https://goo.gl/maps/FHADwznVmfR2", Address= "51300 US-60", City="Aguila",Zip= "85320", Description= "The Aguila Library opened in its new building in 2006. The 2,500 sq. ft. facility brings the latest in quality library services to this rural community. It contains a large Spanish materials collection for this farming community.", Facebook= "mcldag", SubscriptionLinkId=0,EventLinkId="0" };
            var centralExp = new Locations {Id=1, Name = "Central Express", BranchCode = "AD", Stub = "centralexpress", MapLink = "https://goo.gl/maps/DbFro1FfiBP2", Address = "2700 N Central Ave Ste 700", City = "Phoenix", Zip = "85004", Description = "The Central Express library is a pick up/drop off (i.e., no collection to browse) location for County Library materials. It's located in midtown Phoenix near Central Ave and Thomas Rd, a convenient stop for people who live, work, or pass through the area to pick up and drop off materials from 8:00am &ndash; 5:00pm, Monday &ndash; Friday. The Central Express library also serves as MCLD's administrative headquarters. This location offers onsite use of iPads and digital services, as well as new library card applications and renewals.", Facebook = "mcldaz", SubscriptionLinkId = null, EventLinkId = "ALL" };
            var edRobson = new Locations {Id=2, Name = "Ed Robson", BranchCode = "RO", Stub = "edrobson", MapLink = "https://goo.gl/maps/amHZCLcsT7T2", Address = "9330 E Riggs Rd", City = "Sun Lakes", Zip = "85248", Description = "The Ed Robson Library moved into its 10,000-sq. ft. facility in July 2004. This library serves Sun Lakes and surrounding communities.", Facebook = "mcldro", SubscriptionLinkId = 1, EventLinkId = "1" };
            var elMirage = new Locations {Id=3, Name = "El Mirage", BranchCode = "EM", Stub = "elmirage", MapLink = "https://goo.gl/maps/gQqWhaBHbAn", Address = "14011 N 1st Ave", City = "El Mirage", Zip = "85335", Description = "The El Mirage Library is housed in a 2500-sq. ft. building. The El Mirage Library serves a city comprised of a culturally deep-rooted Hispanic/Spanish population, as well as a rapidly increasing population of new families from nearby developments. The library has a collection designed to meet the needs of this diverse community and includes a large collection of Spanish materials. The library also has Internet accessible computers and regular bilingual programming activities.", Facebook = "mcldem", SubscriptionLinkId = 2, EventLinkId = "2" };
            Locations[] locations = { aguila, centralExp, edRobson, elMirage };

            var volunteer = new Features {Id=0, Name="Volunteer", FontAwesome= "fa fa-inverse fa-info-circle fa-stack-1x", ImagePath="voluntold.jpg",Stub="Volunteer",BodyText=""};
            var sevenDay = new Features {Id=1, Name = "7-Day Express", FontAwesome = "fa-stack-1x", ImagePath = "day.jpg", Stub = "7Day", BodyText = "Sometimes called “Lucky Day” materials, we reserve a few copies of these popular books that can only be checked out from the branch. So if you find one, it’s your lucky day! Here’s what you need to know:" };
            var citizenSci = new Features {Id=2, Name = "Citizen Science", FontAwesome = "fa fa-inverse fa-flask fa-stack-1x", ImagePath = "citizen.jpg", Stub = "CitizenScience", BodyText = "Citizen Science kits empower our customers to participate in research with instruments checked out from their local library. See which kits are available from the Ed Robson Library. Watch videos about the Zombee Hunting and Bio Diversity kits to learn more." };
            var facebook = new Features {Id=3, Name = "Facebook", FontAwesome = "fa fa-inverse fa-facebook fa-stack-1x", ImagePath = "facebook.jpg", Stub = "", BodyText = "" };
            Features[] features = { volunteer,sevenDay,citizenSci,facebook };

            var aguilaVolunteer = new LocationsFeatures {LocationId=0,FeatureId=0, RedirectUrl= "https://mcldaz.org/volunteer/AG"};
            var edVolunteer = new LocationsFeatures { LocationId = 2, FeatureId = 0, RedirectUrl= "https://mcldaz.org/volunteer/RO"};
            var elVolunteer = new LocationsFeatures { LocationId = 3, FeatureId =0, RedirectUrl= "https://mcldaz.org/volunteer/EM"};

            var aguilaSeven = new LocationsFeatures { LocationId = 0, FeatureId = 1,Text= " - <span class=\"fa - stack - 1x\" style=\"color: white; \"><strong>7</strong></span> 7-day checkout" };
            var edSeven = new LocationsFeatures { LocationId=2,FeatureId=1,Text = " - <span class=\"fa - stack - 1x\" style=\"color: white; \"><strong>7</strong></span> 7-day checkout" };
            var elSeven = new LocationsFeatures { LocationId = 3, FeatureId =1,Text = " - <span class=\"fa - stack - 1x\" style=\"color: white; \"><strong>7</strong></span> 7-day checkout" };

            var edCitizen = new LocationsFeatures { LocationId = 2, FeatureId =2, Text="See which kits are available from the Ed Robson Library."};

            var aguilaFace = new LocationsFeatures { LocationId = 0, FeatureId = 3,RedirectUrl=""};
            var centralFace = new LocationsFeatures { LocationId = 1, FeatureId = 3};
            var edFace = new LocationsFeatures { LocationId = 2, FeatureId =3};
            var elFace = new LocationsFeatures { LocationId = 3, FeatureId =3 };

            LocationsFeatures[] locationsFeat = {aguilaVolunteer,aguilaSeven,aguilaFace,edCitizen,edFace,edSeven,edVolunteer,elVolunteer,elSeven,elFace,centralFace};

            var model = new LocationsViewModel();
            foreach (var location in locations)
            {
                if(location.Stub == stub)
                {
                    model.Location = location;
                    foreach (var item in locationsFeat)
                    {
                        if (item.LocationId == location.Id)
                        {
                            foreach (var feat in features)
                            {
                                if(item.FeatureId == feat.Id)
                                {
                                    var locafeat = new LocationsFeaturesViewModel();
                                    if (feat.Name == "Facebook")
                                    {
                                        if(stub != "hollyhock")
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
                                    model.LocationFeatures.Add(locafeat);
                                }
                            }
                        }
                    }
                    break;
                }
            }

            return View("Locations",model);
        }
    }
}
