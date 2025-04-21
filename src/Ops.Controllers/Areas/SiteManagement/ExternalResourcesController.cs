using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ExternalResources;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class ExternalResourcesController : BaseController<ExternalResourcesController>
    {
        private readonly IExternalResourcePromService _externalResourcePromService;

        public static string Name { get { return "ExternalResources"; } }
        public static string Area { get { return "SiteManagement"; } }

        public ExternalResourcesController(
            ServiceFacades.Controller<ExternalResourcesController> context,
            IExternalResourcePromService externalResourcepromService) : base(context)
        {
            _externalResourcePromService = externalResourcepromService
                ?? throw new ArgumentNullException(nameof(externalResourcepromService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(ExternalResourceType type = ExternalResourceType.CSS)
        {
            var resourceList = await _externalResourcePromService.GetAllAsync(type);

            var viewModel = new IndexViewModel
            {
                ExternalResources = resourceList,
                Type = type
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> Create(ExternalResource resource)
        {
            var success = false;
            var message = string.Empty;

            try
            {
                await _externalResourcePromService.AddAsync(resource);
                ShowAlertSuccess($"Added external resource: {resource.Name}");
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
        public async Task<JsonResult> Edit(ExternalResource resource)
        {
            var success = false;
            var message = string.Empty;

            try
            {
                await _externalResourcePromService.EditAsync(resource);
                ShowAlertSuccess($"Edited external resource: {resource.Name}");
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
        public async Task<JsonResult> Delete(ExternalResource resource)
        {
            var success = false;
            var message = string.Empty;

            try
            {
                await _externalResourcePromService.DeleteAsync(resource.Id);
                ShowAlertSuccess($"Deleted external resource: {resource.Name}");
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
        public async Task<JsonResult> ChangeSort(ExternalResource resource, bool increase)
        {
            var success = false;
            var message = string.Empty;

            try
            {
                if (increase)
                {
                    await _externalResourcePromService.IncreaseSortOrder(resource.Id);
                }
                else
                {
                    await _externalResourcePromService.DecreaseSortOrder(resource.Id);
                }
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
