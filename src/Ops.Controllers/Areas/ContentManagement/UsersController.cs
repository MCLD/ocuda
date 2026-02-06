using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class UsersController : BaseController<UsersController>
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ITitleClassService _titleClassService;
        private readonly IUserMetadataTypeService _userMetadataTypeService;
        private readonly IUserService _userService;

        public UsersController(ServiceFacades.Controller<UsersController> context,
            IApiKeyService apiKeyService,
            IPermissionGroupService permissionGroupService,
            ITitleClassService titleClassService,
            IUserMetadataTypeService userMetadataTypeService,
            IUserService userService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(apiKeyService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(titleClassService);
            ArgumentNullException.ThrowIfNull(userMetadataTypeService);
            ArgumentNullException.ThrowIfNull(userService);

            _apiKeyService = apiKeyService;
            _permissionGroupService = permissionGroupService;
            _titleClassService = titleClassService;
            _userMetadataTypeService = userMetadataTypeService;
            _userService = userService;
        }

        public static string Area
        { get { return "ContentManagement"; } }

        public static string Name
        { get { return "Users"; } }

        [HttpGet("[action]")]
        public async Task<IActionResult> AddApiKey()
        {
            var baseUri = await GetBaseUriBuilderAsync();
            baseUri.Path = Url.Action(nameof(StaffController.SearchJson), StaffController.Name);

            var viewModel = new AddApiKeyViewModel
            {
                JsonStaffSearchUri = baseUri.Uri
            };

            foreach (ApiKeyType apiKeyType in Enum.GetValues(typeof(ApiKeyType)))
            {
                viewModel.ApiKeyTypesSelectList.Add(new SelectListItem(
                    $"{apiKeyType} - {apiKeyType.GetDescriptionAttributeText()}",
                    apiKeyType.ToString()));
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddApiKey(AddApiKeyViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            string apiKey = null;

            try
            {
                apiKey = await _apiKeyService.CreateAsync(viewModel.ApiKeyType,
                    viewModel.ActAsUserId,
                    viewModel.EndDate);
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Unable to create API key for {UserId}: {ErrorMessage}",
                    CurrentUsername,
                    oex.Message);
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                ShowAlertDanger("Unable to create API key");
            }
            else
            {
                var userInfo = await _userService.GetByIdAsync(viewModel.ActAsUserId);
                ShowAlertWarning($"API Key generated to act as user <strong>{userInfo.Name} ({userInfo.Username})</strong>. Please note this API key, it will not be displayed again: <strong>{apiKey}</strong>");
            }

            return RedirectToAction(nameof(ApiKeys));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddTitle(int titleClassId,
            string titleClassName,
            string titleIsUpdate,
            string title)
        {
            if (string.IsNullOrEmpty(titleIsUpdate))
            {
                if (string.IsNullOrEmpty(titleClassName))
                {
                    ShowAlertWarning("Unable to add title classification: you must specify a name.");
                    return RedirectToAction(nameof(TitleClassList));
                }

                if (string.IsNullOrEmpty(title))
                {
                    ShowAlertWarning("Unable to add title classification: you must select a title.");
                    return RedirectToAction(nameof(TitleClassList));
                }

                titleClassId = await _titleClassService
                    .NewTitleClassificationAsync(titleClassName, title);
            }
            else
            {
                if (string.IsNullOrEmpty(title))
                {
                    ShowAlertWarning("Unable to add title classification: you must select a title.");
                    return RedirectToAction(nameof(TitleClassList));
                }

                var titleClass = await _titleClassService.GetAsync(titleClassId);
                if (titleClass == null)
                {
                    ShowAlertWarning($"Unable to find title classification id {titleClassId}.");
                    return RedirectToAction(nameof(TitleClassList));
                }

                try
                {
                    await _titleClassService.AddTitleAsync(titleClassId, title);
                }
                catch (OcudaException oex)
                {
                    ShowAlertDanger(oex.Message);
                }
            }

            return RedirectToAction(nameof(TitleClassDetails), new { titleClassId });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ApiKeys(int page)
        {
            int currentPage = page < 2 ? 1 : page;

            var filter = new BaseFilter(currentPage);

            var apiKeys = await _apiKeyService.PageAsync(filter);

            var viewModel = new ApiKeysViewModel
            {
                CurrentPage = currentPage,
                ItemCount = apiKeys.Count,
                ItemsPerPage = filter.Take.Value
            };

            ((List<ApiKey>)viewModel.ApiKeys).AddRange(apiKeys.Data);

            if (viewModel.PastMaxPage)
            {
                viewModel.CurrentPage = viewModel.MaxPage;
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> CreateMetadataType(UserMetadataType metadataType)
        {
            ArgumentNullException.ThrowIfNull(metadataType);

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
        public async Task<JsonResult> CreatePermissionGroup(PermissionGroup permissionGroup)
        {
            ArgumentNullException.ThrowIfNull(permissionGroup);

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

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteApiKey(int apiKeyId)
        {
            try
            {
                await _apiKeyService.DeleteAsync(apiKeyId);
                ShowAlertInfo("API key deleted.");
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Unable to delete API key: {ErrorMessage}",
                    oex.Message);
                ShowAlertDanger("Unable to delete API key.");
            }

            return RedirectToAction(nameof(ApiKeys));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> DeleteMetadataType(UserMetadataType metadataType)
        {
            ArgumentNullException.ThrowIfNull(metadataType);

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

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> DeletePermissionGroup(PermissionGroup permissionGroup)
        {
            ArgumentNullException.ThrowIfNull(permissionGroup);

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
                var assignedGroups = ex.Data["Assigned"] as ICollection<string>;
                if (assignedGroups?.Count > 0)
                {
                    message = $"This permission group is assigned to application permissions: {string.Join(", ", assignedGroups).Trim()}";
                }
                else
                {
                    message = ex.Message;
                }
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> EditMetadataType(UserMetadataType metadataType)
        {
            ArgumentNullException.ThrowIfNull(metadataType);

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
        public async Task<JsonResult> EditPermissionGroup(PermissionGroup permissionGroup)
        {
            ArgumentNullException.ThrowIfNull(permissionGroup);

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

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveTitle(int titleClassId, string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                ShowAlertWarning("Unable to add title classification: you must select a title.");
                return RedirectToAction(nameof(TitleClassList));
            }

            var titleClass = await _titleClassService.GetAsync(titleClassId);
            if (titleClass == null)
            {
                ShowAlertWarning($"Unable to find title classification id {titleClassId}.");
                return RedirectToAction(nameof(TitleClassList));
            }

            var titleRemoved = await _titleClassService.RemoveTitleAsync(titleClassId, title);

            if (titleRemoved)
            {
                ShowAlertInfo($"Removed title classification: {titleClass.Name}.");
                return RedirectToAction(nameof(TitleClassList));
            }

            return RedirectToAction(nameof(TitleClassDetails), new { titleClassId });
        }

        [HttpGet("[action]/{titleClassId}")]
        public async Task<IActionResult> TitleClassDetails(int titleClassId)
        {
            var viewModel = new TitleClassViewModel
            {
                TitleClass = await _titleClassService.GetAsync(titleClassId)
            };
            var allTitles = await _userService.GetTitlesAsync();
            var titleList = viewModel.TitleClass.TitleClassMappings.Select(_ => _.UserTitle);
            viewModel.Titles.AddRange(allTitles.Where(_ => !titleList.Contains(_)));
            return View(viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TitleClassList(int page)
        {
            int currentPage = page < 2 ? 1 : page;

            var filter = new BaseFilter(currentPage);

            var titleClasses = await _titleClassService.GetPaginatedAsync(filter);

            var viewModel = new TitleClassesViewModel
            {
                CurrentPage = currentPage,
                ItemCount = titleClasses.Count,
                ItemsPerPage = filter.Take.Value
            };

            viewModel.TitleClasses.AddRange(titleClasses.Data);

            if (viewModel.PastMaxPage)
            {
                viewModel.CurrentPage = viewModel.MaxPage;
            }
            viewModel.Titles.AddRange(await _userService.GetTitlesAsync());
            return View(viewModel);
        }
    }
}