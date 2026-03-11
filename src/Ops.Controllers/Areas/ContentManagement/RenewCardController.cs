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

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class RenewCardController : BaseController<RenewCardController>
    {
        private readonly IEmailService _emailService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IRenewCardService _renewCardService;

        public RenewCardController(ServiceFacades.Controller<RenewCardController> context,
            IEmailService emailService,
            ILanguageService languageService,
            IPermissionGroupService permissionGroupService,
            IRenewCardService renewCardService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(renewCardService);

            _emailService = emailService;
            _languageService = languageService;
            _permissionGroupService = permissionGroupService;
            _renewCardService = renewCardService;
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
        public async Task<IActionResult> CreateResponse(RenewCardResponsesViewModel viewModel)
        {
            if (!await HasRenewCardManagementRightsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(viewModel);

            var response = await _renewCardService.CreateResponseAsync(viewModel.Response);

            return RedirectToAction(nameof(RenewalResponse), new { id = response.Id });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteResponse(RenewCardResponsesViewModel viewModel)
        {
            if (!await HasRenewCardManagementRightsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(viewModel);

            await _renewCardService.DeleteResponseAsync(viewModel.Response.Id);

            ShowAlertSuccess($"Deleted response: {viewModel.Response.Name}");

            return RedirectToAction(nameof(RenewalResponses));
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
        public async Task<IActionResult> RenewalResponse(int id)
        {
            if (!await HasRenewCardManagementRightsAsync()) { return RedirectToUnauthorized(); }

            var response = await _renewCardService.GetResponseAsync(id);
            if (response == null)
            {
                ShowAlertDanger($"Could not find Response with ID: {id}");
                return RedirectToAction(nameof(RenewalResponses));
            }

            var emailSetups = await _emailService.GetEmailSetupsAsync();

            var viewModel = new RenewCardResponseViewModel
            {
                Response = response,
                EmailSetups = new SelectList(emailSetups, "Key", "Value"),
                Languages = await _languageService.GetActiveAsync()
            };

            if (response.EmailSetupId.HasValue && viewModel.Languages.Any())
            {
                viewModel.EmailSetupText = await _emailService.GetSetupTextByLanguageAsync(
                    response.EmailSetupId.Value,
                    viewModel.Languages.FirstOrDefault()?.Name);
            }

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> RenewalResponse(int id, RenewCardResponseViewModel viewModel)
        {
            if (!await HasRenewCardManagementRightsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(viewModel?.Response);

            viewModel.Response.Id = id;

            if (ModelState.IsValid)
            {
                try
                {
                    await _renewCardService.UpdateResponseAsync(viewModel.Response);
                    ShowAlertSuccess("Updated response");
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex, "Error updating card renewal response: {Message}",
                        ex.Message);
                    ShowAlertDanger("Error updating response");
                }
            }

            return RedirectToAction(nameof(RenewalResponse), new { id = viewModel.Response.Id });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RenewalResponses()
        {
            if (!await HasRenewCardManagementRightsAsync())
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new RenewCardResponsesViewModel
            {
                Responses = await _renewCardService.GetResponsesAsync()
            };

            return View(viewModel);
        }

        private async Task<bool> HasRenewCardManagementRightsAsync()
            => await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.RenewCardManagement);
    }
}