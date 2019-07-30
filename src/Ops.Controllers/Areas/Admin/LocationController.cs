using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
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
        private readonly IHtmlGenerator _htmlGenerator;

        public LocationController(ServiceFacades.Controller<LocationController> context,
    ILocationService locationService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index(string locationStub)
        {
            if (string.IsNullOrEmpty(locationStub))
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
            else
            {
                
                var formGroup = new FormGroupTagHelper(_htmlGenerator);
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                var viewModel = new LocationViewModel
                {
                    Location = location,
                };

                return View("LocationDetails",viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLocation()
        {
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateLocation()
        {
            return View("Index");
        }
    }
}
