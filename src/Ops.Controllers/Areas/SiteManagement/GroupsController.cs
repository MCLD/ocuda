﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Group;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class GroupsController : BaseController<GroupsController>
    {
        private readonly IGroupService _groupService;
        private readonly ILocationService _locationService;

        public static string Name { get { return "Groups"; } }

        public GroupsController(ServiceFacades.Controller<GroupsController> context,
            ILocationService locationService,
            IGroupService groupService) : base(context)
        {
            _locationService = locationService
            ?? throw new ArgumentNullException(nameof(locationService));
            _groupService = groupService
                ?? throw new ArgumentNullException(nameof(groupService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var groupList = await _groupService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = groupList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new
                {
                    page = paginateModel.LastPage ?? 1
                });
            }

            return View(new GroupViewModel
            {
                AllGroups = groupList.Data,
                PaginateModel = paginateModel
            });
        }

        [HttpGet("{groupStub}")]
        [RestoreModelState]
        public async Task<IActionResult> Groups(string groupStub)
        {
            try
            {
                var group = await _groupService.GetGroupByStubAsync(groupStub);
                group.IsNewGroup = false;
                group.IsLocationRegion = !string.IsNullOrEmpty(group.SubscriptionUrl);
                return View("GroupDetails", new GroupViewModel
                {
                    Group = group,
                    Action = nameof(GroupsController.EditGroup),
                    AllLocations = await _locationService.GetAllLocationsAsync(),
                    SelectLocations = new SelectList(await _locationService.GetAllLocationsAsync(), "Id", "Name")
                });
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Group {groupStub}: {ex.Message}");
                return RedirectToAction(nameof(GroupsController.Index));
            }
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> CreateGroup()
        {
            var group = new Group
            {
                IsNewGroup = true
            };

            return View("GroupDetails", new GroupViewModel
            {
                Group = group,
                Action = nameof(GroupsController.CreateGroup),
                AllLocations = await _locationService.GetAllLocationsAsync(),
                SelectLocations = new SelectList(await _locationService.GetAllLocationsAsync(), "Id", "Name")
        });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateGroup(Group group)
        {
            if (group.IsLocationRegion && string.IsNullOrEmpty(group.SubscriptionUrl))
            {
                ModelState.AddModelError("Group.SubscriptionUrl", "A 'Subscription URL' is required for a location region.");
                ShowAlertDanger("A 'Subscription URL' is required for a location region.");
                group.IsNewGroup = true;
                return RedirectToAction(nameof(CreateGroup));
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _groupService.AddGroupAsync(group);
                    ShowAlertSuccess($"Added Group: {group.GroupType}");
                    group.IsNewGroup = true;
                    return RedirectToAction(nameof(GroupsController.Groups), new { groupStub = group.Stub });
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex,
                        "Problem creating Group {GroupType}: {Message}",
                        group.GroupType,
                        ex.Message);
                    ShowAlertDanger($"Unable to create Group: {ex.Message}");
                    group.IsNewGroup = true;
                    return View("GroupDetails", new GroupViewModel
                    {
                        Group = group,
                        Action = nameof(GroupsController.CreateGroup)
                    });
                }
            }
            else
            {
                ShowAlertDanger("Invalid paramaters");
                group.IsNewGroup = true;
                return RedirectToAction(nameof(CreateGroup));
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteGroup(Group group)
        {
            try
            {
                await _groupService.DeleteAsync(group.Id);
                ShowAlertSuccess($"Deleted Group: {group.GroupType}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to delete Group {group.GroupType}: {ex.Message}");
                _logger.LogError(ex,
                    "Problem deleting Group {GroupType}: {Message}",
                    group.GroupType,
                    ex.Message);
            }

            return RedirectToAction(nameof(GroupsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditGroup(Group group)
        {
            if (group.IsLocationRegion && string.IsNullOrEmpty(group.SubscriptionUrl))
            {
                ModelState.AddModelError("Group.SubscriptionUrl", "A 'Subscription URL' is required for a location region.");
                ShowAlertDanger($"A 'Subscription URL' is required for a location region.");
                group.IsNewGroup = false;
                return View("GroupDetails", new GroupViewModel
                {
                    Group = group,
                    Action = nameof(GroupsController.EditGroup)
                });
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _groupService.EditAsync(group);
                    ShowAlertSuccess($"Updated group: {group.GroupType}");
                    group.IsNewGroup = false;
                    return RedirectToAction(nameof(GroupsController.Groups), new { groupStub = group.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to update Group {group.GroupType} : {ex.Message}");
                    _logger.LogError(ex,
                        "Problem updating {GroupType}: {Message}",
                        group.GroupType,
                        ex.Message);
                    group.IsNewGroup = false;
                    return View("GroupDetails", new GroupViewModel
                    {
                        Group = group,
                        Action = nameof(GroupsController.EditGroup)
                    });
                }
            }
            return RedirectToAction(nameof(GroupsController.Groups), new { groupStub = group.Stub });
        }
    }
}
