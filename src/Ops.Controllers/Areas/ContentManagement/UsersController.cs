using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class UsersController : BaseController<UsersController>
    {
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserMetadataTypeService _userMetadataTypeService;

        public static string Name { get { return "Users"; } }
        public static string Area { get { return "ContentManagement"; } }

        public UsersController(ServiceFacades.Controller<UsersController> context,
            IPermissionGroupService permissionGroupService,
            IUserMetadataTypeService userMetadataTypeService) : base(context)
        {
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _userMetadataTypeService = userMetadataTypeService
                ?? throw new ArgumentNullException(nameof(userMetadataTypeService));
        }

        [Route("[action]")]
        public async Task<IActionResult> Metadata(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var metadataTypeList = await _userMetadataTypeService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = metadataTypeList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = paginateModel.LastPage ?? 1 });
            }

            return View(new MetadataViewModel
            {
                MetadataTypes = metadataTypeList.Data,
                PaginateModel = paginateModel
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> CreateMetadataType(UserMetadataType metadataType)
        {
            if(metadataType == null)
            {
                throw new ArgumentNullException(nameof(metadataType));
            }

            var success = false;
            var message = string.Empty;

            try
            {
                var newMetadataType = await _userMetadataTypeService.AddAsync(metadataType);
                ShowAlertSuccess($"Added user metadata type: {newMetadataType.Name}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> EditMetadataType(UserMetadataType metadataType)
        {
            if (metadataType == null)
            {
                throw new ArgumentNullException(nameof(metadataType));
            }

            var success = false;
            var message = string.Empty;

            try
            {
                var updatedMetadataType = await _userMetadataTypeService.EditAsync(metadataType);
                ShowAlertSuccess($"Edited user metadata type: {updatedMetadataType.Name}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> DeleteMetadataType(UserMetadataType metadataType)
        {
            if (metadataType == null)
            {
                throw new ArgumentNullException(nameof(metadataType));
            }

            var success = false;
            var message = string.Empty;

            try
            {
                await _userMetadataTypeService.DeleteAsync(metadataType.Id);
                ShowAlertSuccess($"Deleted user metadata type: {metadataType.Name}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [Route("[action]")]
        public async Task<IActionResult> Permissions(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var permissionList = await _permissionGroupService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = permissionList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = paginateModel.LastPage ?? 1 });
            }

            return View(new PermissionViewModel
            {
                PermissionGroups = permissionList.Data,
                PaginateModel = paginateModel
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> CreatePermissionGroup(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
            {
                throw new ArgumentNullException(nameof(permissionGroup));
            }

            var success = false;
            var message = string.Empty;

            try
            {
                var newPermissionGroup = await _permissionGroupService.AddAsync(permissionGroup);
                ShowAlertSuccess($"Added new permission group: {newPermissionGroup.PermissionGroupName}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> EditPermissionGroup(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
            {
                throw new ArgumentNullException(nameof(permissionGroup));
            }

            var success = false;
            var message = string.Empty;

            try
            {
                var updatedPermissionGroup
                    = await _permissionGroupService.EditAsync(permissionGroup);
                ShowAlertSuccess($"Edited permission group: {updatedPermissionGroup.PermissionGroupName}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> DeletePermissionGroup(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
            {
                throw new ArgumentNullException(nameof(permissionGroup));
            }

            var success = false;
            var message = string.Empty;

            try
            {
                await _permissionGroupService.DeleteAsync(permissionGroup.Id);
                ShowAlertSuccess($"Deleted permission group: {permissionGroup.PermissionGroupName}");
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }
    }
}
