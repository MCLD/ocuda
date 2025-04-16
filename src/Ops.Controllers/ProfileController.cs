using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Profile;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public partial class ProfileController : BaseController<ProfileController>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserManagementService _userManagementService;
        private readonly IUserService _userService;

        public ProfileController(ServiceFacades.Controller<ProfileController> context,
            IHttpContextAccessor httpContext,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            IUserManagementService userManagementService,
            IUserService userService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(userManagementService);
            ArgumentNullException.ThrowIfNull(userService);

            _httpContext = httpContext;
            _locationService = locationService;
            _permissionGroupService = permissionGroupService;
            _userManagementService = userManagementService;
            _userService = userService;
        }

        public static string Name
        { get { return "Profile"; } }

        [HttpPost("[action]")]
        public async Task<IActionResult> EditNickname(IndexViewModel model)
        {
            if (model?.User.Id != CurrentUserId)
            {
                return RedirectToUnauthorized();
            }
            if (ModelState.IsValid && model != null)
            {
                try
                {
                    var user = await _userManagementService.EditNicknameAsync(model.User);
                    ShowAlertSuccess($"Updated nickname: {user.Nickname}");
                }
                catch (OcudaException oex)
                {
                    ShowAlertDanger("Unable to update nickname: ", oex.Message);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var viewModel = new IndexViewModel
            {
                CanUpdatePicture = IsSiteManager(),
                CanViewLastSeen = IsSiteManager(),
                Locations = await GetLocationsDropdownAsync(_locationService),
                UserViewingSelf = string.IsNullOrEmpty(id)
                    || id == UserClaim(ClaimType.Username)
            };

            if (!viewModel.CanUpdatePicture)
            {
                viewModel.CanUpdatePicture = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.UpdateProfilePictures);
            }

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

            if (!string.IsNullOrEmpty(viewModel.User.PictureFilename))
            {
                viewModel.PicturePath = Url.Action(nameof(Picture),
                    new { id = id ?? UserClaim(ClaimType.Username) });
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

            viewModel.RelatedTitleClassifications
                = await _userService.GetRelatedTitleClassificationsAsync(viewModel.User.Id);

            return View(viewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Picture(string id)
        {
            var picture = await _userService.GetProfilePictureAsync(id);

            if (picture == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            Response.Headers.Add("Content-Disposition", "inline; filename=" + picture.Filename);
            return File(picture.FileData, picture.FileType);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Reauthenticate()
        {
            await _httpContext.HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemovePicture(int userId, string username)
        {
            if (!IsSiteManager())
            {
                var picturePermission = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.UpdateProfilePictures);
                if (!picturePermission)
                {
                    return RedirectToUnauthorized();
                }
            }

            await _userManagementService.RemoveProfilePictureAsync(userId);
            return RedirectToAction(nameof(Index), new { id = username });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UnsetManualLocation(int userId)
        {
            if (userId != CurrentUserId)
            {
                return RedirectToUnauthorized();
            }

            await _userManagementService.UnsetManualLocationAsync(userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateLocation(int userId, int locationId)
        {
            if (userId != CurrentUserId)
            {
                return RedirectToUnauthorized();
            }
            await _userManagementService.UpdateLocationAsync(userId, locationId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> UpdatePicture(int id)
        {
            if (!IsSiteManager())
            {
                var picturePermission = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.UpdateProfilePictures);
                if (!picturePermission)
                {
                    return RedirectToUnauthorized();
                }
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                ShowAlertDanger("Unable to find that user.");
                return RedirectToAction(nameof(Index));
            }

            return View(new UpdatePictureViewModel
            {
                CropHeight = 700,
                CropWidth = 700,
                DisplayDimension = 700,
                User = user
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult>
            UploadPicture(UpdatePictureViewModel updatePictureViewModel)
        {
            if (!IsSiteManager())
            {
                var picturePermission = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.UpdateProfilePictures);
                if (!picturePermission)
                {
                    return RedirectToUnauthorized();
                }
            }

            if (updatePictureViewModel == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userService.GetByIdAsync(updatePictureViewModel.UserId);

            if (user == null)
            {
                ShowAlertDanger("Unable to find that user.");
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(updatePictureViewModel.ProfilePicture))
            {
                ShowAlertWarning("You must upload a file to replace a profile image.");
                return RedirectToAction(nameof(Index), new { id = user.Username });
            }

            try
            {
                await _userManagementService
                    .UploadProfilePictureAsync(user, updatePictureViewModel.ProfilePicture);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger("Problem with upload: " + oex.Message);
            }

            return RedirectToAction(nameof(Index), new { id = user.Username });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> BatchUploadPictures()
        {
            if (!IsSiteManager())
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BatchUploadPicturesViewModel();
            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> BatchUploadPictures(BatchUploadPicturesViewModel viewModel)
        {
            if (!IsSiteManager())
            {
                return Json(new
                {
                    success = false,
                    message = "Only users with site manager privileges can batch upload profile pictures"
                });
            }

            if (viewModel != null)
            {
                Models.Entities.User user = null;

                var allUsers = await _userService.GetAllUsersAsync();

                var lastNameUsers = allUsers.Where(u => NormalizeString(u.Name).Contains(viewModel.LastName, StringComparison.OrdinalIgnoreCase));

                if (lastNameUsers.Count() == 1)
                {
                    user = lastNameUsers.First();
                }
                else if (lastNameUsers.Count() > 1)
                {
                    var fullNameUsers = lastNameUsers.Where(u => NormalizeString(u.Name).Contains(viewModel.FirstName, StringComparison.OrdinalIgnoreCase));

                    user = fullNameUsers.Count() == 1 ? fullNameUsers.First() : null;
                }

                if (user == null)
                {
                    try
                    {
                        var location = await _locationService.GetLocationByCodeAsync(viewModel.LocationCode);
                        var locationUsers = lastNameUsers.Where(u => u.AssociatedLocation == location?.Id);

                        if (locationUsers.Count() == 1)
                        {
                            user = locationUsers.First();
                        }
                        else if (locationUsers.Count() > 1)
                        {
                            var fullNameLocation = locationUsers.Where(u => u.Name.Contains(viewModel.FirstName, StringComparison.OrdinalIgnoreCase));
                            user = fullNameLocation.Count() == 1 ? fullNameLocation.First() : null;
                        }
                    }
                    catch (OcudaException ex)
                    {
                        Console.WriteLine(ex.Message + " Location code: " + viewModel.LocationCode);
                    }
                }

                if (user != null)
                {
                    try
                    {
                        await _userManagementService
                            .UploadProfilePictureAsync(user, viewModel.ProfilePicture);
                        return Json(new { success = true, message = "Picture uploaded successfully" });
                    }
                    catch (OcudaException oex)
                    {
                        ShowAlertDanger("Problem with upload: " + oex.Message);
                    }
                }
            }

            return Json(new { success = false, message = $"The provided photo for {viewModel?.FirstName} {viewModel?.LastName} could not be matched with a user profile." });
        }

        // For comparing photo filenames to database user names, remove apostrophes, accent marks, etc.
        private static string NormalizeString(string text)
        {
            return RemoveNonAlphaChars(RemoveDiacritics(text));
        }

        private static string RemoveNonAlphaChars(string text)
        {
            return NonAlphaCharsRegex().Replace(text, "");
        }

        private static string RemoveDiacritics(string text)
        {
            return string.Concat(
                text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                                              UnicodeCategory.NonSpacingMark)
              ).Normalize(NormalizationForm.FormC);
        }

        [GeneratedRegex("[^a-zA-Z]")]
        private static partial Regex NonAlphaCharsRegex();
    }
}