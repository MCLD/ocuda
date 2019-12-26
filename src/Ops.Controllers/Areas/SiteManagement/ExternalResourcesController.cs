using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ExternalResources;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
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
        private readonly IExternalResourceService _externalResourceService;

        public ExternalResourcesController(
            ServiceFacades.Controller<ExternalResourcesController> context,
            IExternalResourceService externalResourceService) : base(context)
        {
            _externalResourceService = externalResourceService
                ?? throw new ArgumentNullException(nameof(externalResourceService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(ExternalResourceType type = ExternalResourceType.CSS)
        {
            var resourceList = await _externalResourceService.GetAllAsync(type);

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
                await _externalResourceService.AddAsync(resource);
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
                await _externalResourceService.EditAsync(resource);
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
                await _externalResourceService.DeleteAsync(resource.Id);
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
                    await _externalResourceService.IncreaseSortOrder(resource.Id);
                }
                else
                {
                    await _externalResourceService.DecreaseSortOrder(resource.Id);
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
