using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Staff;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class StaffController : BaseController<StaffController>
    {
        private const int PerPage = 10;

        private readonly ILocationService _locationService;
        private readonly IUserService _userService;

        public StaffController(ServiceFacades.Controller<StaffController> context,
            ILocationService locationService,
            IUserService userService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(userService);

            _locationService = locationService;
            _userService = userService;
        }

        public static string Name
        { get { return "Staff"; } }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index(string searchText, int page, int associatedLocation)
        {
            int currentPage = page < 2 ? 1 : page;

            var filter = new StaffSearchFilter(currentPage, PerPage)
            {
                AssociatedLocation = associatedLocation,
                MustHaveName = true,
                SearchText = searchText
            };

            var users = await _userService.FindAsync(filter);

            var viewModel = new SearchViewModel
            {
                CurrentPage = currentPage,
                ItemCount = users.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = await GetLocationsAsync(_locationService),
                Users = users.Data,
                SearchText = searchText,
                AssociatedLocation = associatedLocation
            };

            if (viewModel.PastMaxPage)
            {
                viewModel.CurrentPage = viewModel.MaxPage;
            }

            return View(viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SearchJson(string searchText, int page)
        {
            int currentPage = page < 2 ? 1 : page;

            var filter = new StaffSearchFilter(currentPage, 5)
            {
                MustHaveName = true,
                SearchText = searchText
            };

            var users = await _userService.FindAsync(filter);

            var result = new SearchViewModel
            {
                CurrentPage = currentPage,
                ItemCount = users.Count,
                ItemsPerPage = filter.Take.Value,
                Users = [.. users.Data.Select(_ => new Models.Entities.User
                {
                    Id = _.Id,
                    Name = _.Name
                })],
                SearchText = searchText
            };

            if (result.PastMaxPage)
            {
                result.CurrentPage = result.MaxPage;
            }

            return Json(result);
        }
    }
}