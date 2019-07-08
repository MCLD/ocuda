using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Users;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class UsersController : BaseController<UsersController>
    {
        private readonly IUserMetadataTypeService _userMetadataTypeService;

        public UsersController(ServiceFacades.Controller<UsersController> context,
            IUserMetadataTypeService userMetadataTypeService) : base(context)
        {
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

            var paginateModel = new PaginateModel()
            {
                ItemCount = metadataTypeList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new MetadataViewModel
            {
                MetadataTypes = metadataTypeList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> CreateMetadataType(UserMetadataType metadataType)
        {
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
    }
}
