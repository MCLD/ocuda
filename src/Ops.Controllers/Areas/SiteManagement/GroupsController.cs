using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILocationGroupService _locationGroupService;

        public static string Name { get { return "Groups"; } }
        public static string Area { get { return "SiteManagement"; } }

        public GroupsController(ServiceFacades.Controller<GroupsController> context,
            ILocationGroupService locationGroupService,
            IGroupService groupService) : base(context)
        {
            _locationGroupService = locationGroupService
            ?? throw new ArgumentNullException(nameof(locationGroupService));
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
                var viewModel = new GroupViewModel
                {
                    Group = group,
                    Action = nameof(GroupsController.EditGroup)
                };
                if (group.IsLocationRegion)
                {
                    viewModel.LocationGroups = await _groupService.GetLocationGroupsByGroupId(group.Id);
                }
                return View("GroupDetails", viewModel);
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Group {groupStub}: {ex.Message}");
                return RedirectToAction(nameof(GroupsController.Index));
            }
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public IActionResult CreateGroup()
        {
            var group = new Group
            {
                IsNewGroup = true
            };

            return View("GroupDetails", new GroupViewModel
            {
                Group = group,
                Action = nameof(GroupsController.CreateGroup)
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
        public async Task<IActionResult> EditGroup(GroupViewModel viewModel)
        {
            if (viewModel.Group.IsLocationRegion && string.IsNullOrEmpty(viewModel.Group.SubscriptionUrl))
            {
                ModelState.AddModelError("Group.SubscriptionUrl", "A 'Subscription URL' is required for a location region.");
                ShowAlertDanger($"A 'Subscription URL' is required for a location region.");
                viewModel.Group.IsNewGroup = false;
                viewModel.Action = nameof(GroupsController.EditGroup);
                return View("GroupDetails", viewModel);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _groupService.EditAsync(viewModel.Group);
                    if (!string.IsNullOrWhiteSpace(viewModel.OrderedLocationIds) && viewModel.Group.IsLocationRegion)
                    {
                        var locationIds = viewModel.OrderedLocationIds
                            .Split(",")
                            .Select(int.Parse)
                            .ToList();
                        var order = 1;
                        foreach (var id in locationIds)
                        {
                            var locationGroup = await _locationGroupService.GetByIdsAsync(viewModel.Group.Id, id);
                            locationGroup.DisplayOrder = order;
                            await _locationGroupService.EditAsync(locationGroup);
                            order++;
                        }
                    }
                    ShowAlertSuccess($"Updated group: {viewModel.Group.GroupType}");
                    viewModel.Group.IsNewGroup = false;
                    viewModel.LocationGroups = await _groupService.GetLocationGroupsByGroupId(viewModel.Group.Id);
                    return RedirectToAction(nameof(GroupsController.Groups), new { groupStub = viewModel.Group.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to update Group {viewModel.Group.GroupType} : {ex.Message}");
                    _logger.LogError(ex,
                        "Problem updating {GroupType}: {Message}",
                        viewModel.Group.GroupType,
                        ex.Message);
                    viewModel.Group.IsNewGroup = false;
                    if (viewModel.Group.IsLocationRegion)
                    {
                        viewModel.LocationGroups = await _groupService.GetLocationGroupsByGroupId(viewModel.Group.Id);
                    }
                    return View("GroupDetails", new GroupViewModel
                    {
                        Group = viewModel.Group,
                        Action = nameof(GroupsController.EditGroup)
                    });
                }
            }
            return RedirectToAction(nameof(GroupsController.Groups), new { groupStub = viewModel.Group.Stub });
        }
    }
}
