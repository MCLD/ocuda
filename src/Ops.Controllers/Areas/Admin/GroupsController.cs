using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Group;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class GroupsController: BaseController<GroupsController>
    {
        private readonly IGroupService _groupService;
        private readonly IConfiguration _config;
        public static string Name { get { return "Groups"; } }

        public GroupsController(IConfiguration config,
            ServiceFacades.Controller<GroupsController> context,
            IGroupService groupService) : base(context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
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

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new GroupViewModel
            {
                AllGroups = groupList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }
        [HttpGet("{groupStub}")]
        [RestoreModelState]
        public async Task<IActionResult> Groups(string groupStub)
        {
            try
            {
                var group = await _groupService.GetGroupByStubAsync(groupStub);
                group.IsNewGroup = false;
                var viewModel = new GroupViewModel
                {
                    Group = group,
                Action = nameof(GroupsController.EditGroup)
                };
                return View("GroupDetails", viewModel);
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Group {groupStub}: {ex.Message}");
                return RedirectToAction(nameof(GroupsController.Index));
            }
        }
        [HttpGet("[action]")]
        [SaveModelState]
        public IActionResult AddGroup()
        {
            var group = new Group();
            group.IsNewGroup = true;
            var viewModel = new GroupViewModel
            {
                Group = group,
                Action = nameof(GroupsController.CreateGroup)
            };

            return View("GroupDetails", viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateGroup(Group group)
        {
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
                    ShowAlertDanger($"Unable to create Group: {ex.Message}");
                    group.IsNewGroup = true;
                    var viewModel = new GroupViewModel
                    {
                        Group = group,
                        Action = nameof(GroupsController.CreateGroup)
                    };

                    return View("GroupDetails", viewModel);
                }
            }
            return RedirectToAction(nameof(GroupsController.AddGroup));
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
            }

            return RedirectToAction(nameof(GroupsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditGroup(Group group)
        {
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
                    group.IsNewGroup = false;
                    var viewModel = new GroupViewModel
                    {
                        Group = group,
                        Action = nameof(GroupsController.EditGroup)
                    };

                    return View("GroupDetails", viewModel);
                }
            }
            return RedirectToAction(nameof(GroupsController.Groups), new { groupStub = group.Stub });
        }
    }
}
