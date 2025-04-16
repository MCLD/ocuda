using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.ApplicationPermissions;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class ApplicationPermissionsController : BaseController<ApplicationPermissionsController>
    {
        private readonly IPermissionGroupService _permissionGroupService;

        public static string Name
        { get { return "ApplicationPermissions"; } }
        public static string Area
        { get { return "ContentManagement"; } }

        public ApplicationPermissionsController(
            ServiceFacades.Controller<ApplicationPermissionsController> context,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var applicationPermissions =
                new List<DataWithCount<ApplicationPermissionDefinition>>();

            foreach (var definition in ApplicationPermissionDefinitions.ApplicationPermissions)
            {
                applicationPermissions.Add(new DataWithCount<ApplicationPermissionDefinition>
                {
                    Count = await _permissionGroupService
                        .GetApplicationPermissionGroupCountAsync(definition.Id),
                    Data = definition
                });
            }

            var viewModel = new IndexViewModel
            {
                ApplicationPermissions = applicationPermissions
            };

            return View(viewModel);
        }

        [Route("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var permission = ApplicationPermissionDefinitions.ApplicationPermissions
                .SingleOrDefault(_ => string.Equals(_.Id, id, StringComparison.OrdinalIgnoreCase));

            if (permission == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var assignedPermissionGroups = await _permissionGroupService
                .GetApplicationPermissionGroupsAsync(permission.Id);

            var viewModel = new DetailViewModel
            {
                ApplicationPermission = permission,
                AssignedGroups = assignedPermissionGroups,
                AvailableGroups = permissionGroups
                    .Where(_ => !assignedPermissionGroups.Select(_ => _.Id).Contains(_.Id))
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddPermissionGroup(DetailViewModel model)
        {
            try
            {
                await _permissionGroupService.AddApplicationPermissionGroupAsync(
                    model.ApplicationPermission.Id,
                    model.PermissionGroupId);

                AlertInfo = "Application permission added.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(Detail), new { id = model.ApplicationPermission.Id });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RemovePermissionGroup(DetailViewModel model)
        {
            try
            {
                await _permissionGroupService.RemoveApplicationPermissionGroupAsync(
                    model.ApplicationPermission.Id,
                    model.PermissionGroupId);

                AlertInfo = "Application permission removed.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem removing permission: {ex.Message}";
            }

            return RedirectToAction(nameof(Detail), new { id = model.ApplicationPermission.Id });
        }
    }
}