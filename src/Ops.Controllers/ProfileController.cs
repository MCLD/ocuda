using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Profile;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class ProfileController : BaseController<ProfileController>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserService _userService;

        public ProfileController(ServiceFacades.Controller<ProfileController> context,
            IHttpContextAccessor httpContext,
            IPermissionGroupService permissionGroupService,
            IUserService userService) : base(context)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public static string Name { get { return "Profile"; } }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditNickname(IndexViewModel model)
        {
            if (ModelState.IsValid && model != null)
            {
                try
                {
                    var user = await _userService.EditNicknameAsync(model.User);
                    ShowAlertSuccess($"Updated nickname: {user.Nickname}");
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update nickname: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("")]
        [Route("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var viewModel = new IndexViewModel
            {
                UserViewingSelf = string.IsNullOrEmpty(id)
                    || id == UserClaim(ClaimType.Username)
            };

            if (!viewModel.UserViewingSelf)
            {
                var user = await _userService.LookupUserAsync(id);

                if (user != null)
                {
                    viewModel.User = user;
                }
                else
                {
                    ShowAlertDanger("Could not find user with username: ", id);
                    return RedirectToAction(nameof(Index), HomeController.Name);
                }
            }
            else
            {
                viewModel.User = await _userService.GetByIdAsync(CurrentUserId);
            }

            if (viewModel.User.SupervisorId.HasValue)
            {
                viewModel.User.Supervisor =
                    await _userService.GetByIdAsync(viewModel.User.SupervisorId.Value);
            }

            viewModel.DirectReports = await _userService.GetDirectReportsAsync(viewModel.User.Id);

            viewModel.CanEdit = viewModel.User.Id == CurrentUserId;

            if (viewModel.UserViewingSelf)
            {
                viewModel.AuthenticatedAt = DateTime.Parse(UserClaim(ClaimType.AuthenticatedAt),
                    CultureInfo.InvariantCulture);

                viewModel.Permissions = new List<string>();

                if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
                {
                    viewModel.Permissions.Add("Site manager");
                }

                var permissionClaims = UserClaims(ClaimType.PermissionId);

                if (permissionClaims?.Count > 0)
                {
                    var permissionGroupIds = permissionClaims
                        .Select(_ => int.Parse(_, CultureInfo.InvariantCulture));

                    var permissionLookup = await _permissionGroupService
                        .GetGroupsAsync(permissionGroupIds);

                    var permissionGroups = permissionLookup
                            .Select(_ => _.PermissionGroupName)
                            .OrderBy(_ => _);

                    viewModel.Permissions = viewModel.Permissions
                        .Concat(permissionGroups)
                        .ToList();
                }
            }

            return View(viewModel);
        }

        [Route("[action]")]
        public async Task<IActionResult> Reauthenticate()
        {
            await _httpContext.HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}