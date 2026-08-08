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
    public class UserSyncController(
        Controller<UserSyncController> context,
        ILocationService locationService,
        IPermissionGroupService permissionGroupService,
        IUserSyncService userSyncService)
        : BaseController<UserSyncController>(context)
    {
        public static string Area
        {
            get { return "ContentManagement"; }
        }

        public static string Name
        {
            get { return "UserSync"; }
        }

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

            await userSyncService.UpdateLocationMappingAsync(CurrentUserId, mappingId, locationId);
            return RedirectToAction(nameof(UpdateLocations));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckLocations()
        {
            return !await HasUserSyncRights()
                ? RedirectToUnauthorized()
                : View("ChangeReport", new ChangeReportViewModel
                {
                    AllowUpdateLocations = true,
                    Status = await userSyncService.CheckSyncLocationsAsync(),
                    Title = "Check Locations",
                });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckSync()
        {
            return !await HasUserSyncRights()
                ? RedirectToUnauthorized()
                : View("ChangeReport", new ChangeReportViewModel
                {
                    AllowPerformSync = true,
                    IsApplied = false,
                    Status = await userSyncService.SyncDirectoryAsync(CurrentUserId, false),
                    Title = "Check Sync",
                });
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> ImportDetail(int id)
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            var detail = await userSyncService.GetImportDetailAsync(id);

            return View("ChangeReport", new ChangeReportViewModel
            {
                IsApplied = true,
                Status = detail,
                Subtitle = detail.AsOf.ToString(System.Globalization.CultureInfo.CurrentCulture),
                Title = "Historical sync",
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

            var rosterHeaders = await userSyncService.GetPaginatedHeadersAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = currentPage,
                ItemCount = rosterHeaders.Count,
                ItemsPerPage = filter.Take.Value,
                UserSyncHistories = rosterHeaders.Data,
            };

            return viewModel.PastMaxPage ? RedirectToRoute(new { page = viewModel.LastPage ?? 1 }) : View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PerformSync()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            await userSyncService.SyncDirectoryAsync(CurrentUserId, true);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SyncLocations()
        {
            if (!await HasUserSyncRights())
            {
                return RedirectToUnauthorized();
            }

            await userSyncService.SyncLocationsAsync(CurrentUserId);
            return RedirectToAction("UpdateLocations");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UpdateLocations()
        {
            return !await HasUserSyncRights()
                ? RedirectToUnauthorized()
                : View("Locations", new LocationsViewModel
                {
                    Locations = await locationService.GetAllLocationsIdNameAsync(),
                    Mapping = await userSyncService.GetLocationsAsync(),
                    Summary = "Locations",
                });
        }

        private async Task<bool> HasUserSyncRights()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.UserSync);
        }
    }
}
