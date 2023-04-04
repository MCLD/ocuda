using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.UserSync;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class UserSyncController : BaseController<UserSyncController>
    {
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserSyncService _userSyncService;

        public UserSyncController(Controller<UserSyncController> context,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            IUserSyncService userSyncService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _userSyncService = userSyncService
                ?? throw new ArgumentNullException(nameof(userSyncService));
        }

        public static string Area
        { get { return "ContentManagement"; } }

        public static string Name
        { get { return "UserSync"; } }

        [HttpPost("[action]")]
        public async Task<IActionResult> AdjustMapping(LocationsViewModel viewModel)
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            if (viewModel == null)
            {
                return RedirectToAction(nameof(Index));
            }

            int mappingId = viewModel.IsClear ? viewModel.ClearId : viewModel.UpdateId;
            int? locationId = viewModel.IsClear ? null : viewModel.SelectedLocation;

            await _userSyncService.UpdateLocationMappingAsync(mappingId, locationId);
            return RedirectToAction(nameof(UpdateLocations));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckLocations()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }
            return View("ChangeReport", new ChangeReportViewModel
            {
                AllowUpdateLocations = true,
                Status = await _userSyncService.CheckSyncLocationsAsync(),
                Title = "Check Locations"
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckSync()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }
            return View("ChangeReport", new ChangeReportViewModel
            {
                AllowPerformSync = true,
                IsApplied = false,
                Status = await _userSyncService.SyncDirectoryAsync(false),
                Title = "Check Sync"
            });
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> ImportDetail(int id)
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            var detail = await _userSyncService.GetImportDetailAsync(id);

            return View("ChangeReport", new ChangeReportViewModel
            {
                IsApplied = true,
                Status = detail,
                Subtitle = detail.AsOf.ToString(System.Globalization.CultureInfo.CurrentCulture),
                Title = "Historical sync"
            });
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        [RestoreModelState(Key = "RosterUpload")]
        public async Task<IActionResult> Index(int page)
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new BaseFilter(currentPage);

            var rosterHeaders = await _userSyncService.GetPaginatedHeadersAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = currentPage,
                ItemCount = rosterHeaders.Count,
                ItemsPerPage = filter.Take.Value,
                UserSyncHistories = rosterHeaders.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PerformSync()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            await _userSyncService.SyncDirectoryAsync(true);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SyncLocations()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            await _userSyncService.SyncLocationsAsync();
            return RedirectToAction("UpdateLocations");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UpdateLocations()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            return View("Locations", new LocationsViewModel
            {
                Locations = await _locationService.GetAllLocationsIdNameAsync(),
                Mapping = await _userSyncService.GetLocationsAsync(),
                Summary = "Locations"
            });
        }

        private async Task<bool> HasUserSyncRights()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.UserSync);
        }
    }
}