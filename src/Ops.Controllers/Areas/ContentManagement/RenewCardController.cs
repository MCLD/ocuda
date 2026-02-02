using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.RenewCard;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class RenewCardController : BaseController<RenewCardController>
    {
        private readonly IRenewCardService _renewCardService;
        private readonly IEmailService _emailService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;

        public RenewCardController(ServiceFacades.Controller<RenewCardController> context,
            IRenewCardService renewCardService,
            IEmailService emailService,
            ILanguageService languageService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(renewCardService);
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _renewCardService = renewCardService;
            _emailService = emailService;
            _languageService = languageService;
            _permissionGroupService = permissionGroupService;
        }

        public static string Area
        { get { return nameof(ContentManagement); } }

        public static string Name
        { get { return "RenewCard"; } }

        [HttpPost("[action]")]
        public async Task<JsonResult> ChangeResponseSort(int id, bool increase)
        {
            JsonResponse response;

            if (await HasRenewCardManagementRightsAsync())
            {
                try
                {
                    await _renewCardService.UpdateResponseSortOrderAsync(id, increase);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateResponse(ResponsesViewModel viewModel)
        {
            if (!await HasRenewCardManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            ArgumentNullException.ThrowIfNull(viewModel);

            var response = await _renewCardService.CreateResponseAsync(viewModel.Response);

            return RedirectToAction(nameof(Response), new { id = response.Id });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteResponse(ResponsesViewModel viewModel)
        {
            if (!await HasRenewCardManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            ArgumentNullException.ThrowIfNull(viewModel);

            await _renewCardService.DeleteResponseAsync(viewModel.Response.Id);

            ShowAlertSuccess($"Deleted response: {viewModel.Response.Name}");

            return RedirectToAction(nameof(Responses));
        }

        [HttpGet("[action]")]
        public async Task<JsonResult> GetEmailSetupText(int emailSetupId, string languageName)
        {
            JsonResponse response;

            if (await HasRenewCardManagementRightsAsync())
            {
                try
                {
                    var emailSetupText = await _emailService.GetSetupTextByLanguageAsync(
                        emailSetupId, languageName);

                    return Json(new
                    {
                        success = true,
                        emailSetupText?.Subject,
                        emailSetupText?.Preview,
                        emailSetupText?.BodyText
                    });
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Response(int id)
        {
            if (!await HasRenewCardManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            var response = await _renewCardService.GetResponseAsync(id);
            if (response == null)
            {
                ShowAlertDanger($"Could not find Response with ID: {id}");
                return RedirectToAction(nameof(Responses));
            }

            var emailSetups = await _emailService.GetEmailSetupsAsync();

            var viewModel = new ResponseViewModel
            {
                Response = response,
                EmailSetups = new SelectList(emailSetups, "Key", "Value"),
                Languages = await _languageService.GetActiveAsync()
            };

            if (response.EmailSetupId.HasValue && viewModel.Languages.Count > 0)
            {
                viewModel.EmailSetupText = await _emailService.GetSetupTextByLanguageAsync(
                    response.EmailSetupId.Value,
                    viewModel.Languages.FirstOrDefault()?.Name);
            }

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Response(ResponseViewModel viewModel)
        {
            if (!await HasRenewCardManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            ArgumentNullException.ThrowIfNull(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    await _renewCardService.UpdateResponseAsync(viewModel.Response);
                    ShowAlertSuccess("Updated response");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating card renewal response: {Message}",
                        ex.Message);
                    ShowAlertDanger("Error updating response");
                }
            }

            return RedirectToAction(nameof(Response), new { id = viewModel.Response.Id });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Responses()
        {
            if (!await HasRenewCardManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new ResponsesViewModel
            {
                Responses = await _renewCardService.GetResponsesAsync()
            };

            return View(viewModel);
        }

        private async Task<bool> HasRenewCardManagementRightsAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.RenewCardManagement);
        }
    }
}
