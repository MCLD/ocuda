using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class UserManagementService : BaseService<UserManagementService>, IUserManagementService
    {
        public static readonly string ProfilePicturePath = "profilepicture";

        private readonly IOcudaCache _cache;
        private readonly IPathResolverService _pathResolver;
        private readonly IUserRepository _userRepository;
        private readonly IVolunteerFormService _volunteerFormService;
        private readonly IImageService _imageService;

        public UserManagementService(ILogger<UserManagementService> logger,
            IHttpContextAccessor httpContextAccessor,
            IOcudaCache cache,
            IPathResolverService pathResolver,
            IUserRepository userRepository,
            IVolunteerFormService volunteerFormService,
            IImageService imageService)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(pathResolver);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(volunteerFormService);
            ArgumentNullException.ThrowIfNull(imageService);

            _cache = cache;
            _pathResolver = pathResolver;
            _userRepository = userRepository;
            _volunteerFormService = volunteerFormService;
            _imageService = imageService;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Normalize usernames and emails to lowercase.")]
        public async Task<User> AddUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            user.Username = user.Username?.Trim().ToLowerInvariant();
            user.Email = user.Email?.Trim().ToLowerInvariant();
            user.CreatedAt = DateTime.Now;

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            User createdUser = await _userRepository.FindByUsernameAsync(user.Username);
            createdUser.CreatedBy = createdUser.Id;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return createdUser;
        }

        public async Task<User> EditNicknameAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            User currentUser = await _userRepository.FindAsync(user.Id);
            currentUser.Nickname = user.Nickname;
            currentUser.UpdatedAt = DateTime.Now;
            currentUser.UpdatedBy = GetCurrentUserId();

            _userRepository.Update(currentUser);
            await _userRepository.SaveAsync();
            return currentUser;
        }

        /// <summary>
        /// Ensure the sysadmin user exists.
        /// </summary>
        public async Task<User> EnsureSysadminUserAsync()
        {
            User sysadminUser = await _userRepository.GetSystemAdministratorAsync();
            if (sysadminUser == null)
            {
                sysadminUser = new User
                {
                    Username = "sysadmin",
                    Name = "System",
                    CreatedAt = DateTime.Now,
                    IsSysadmin = true
                };
                await _userRepository.AddAsync(sysadminUser);
                await _userRepository.SaveAsync();
            }
            if (!sysadminUser.ExcludeFromRoster)
            {
                sysadminUser.ExcludeFromRoster = true;
                _userRepository.Update(sysadminUser);
                await _userRepository.SaveAsync();
            }
            return sysadminUser;
        }

        public async Task LoggedInUpdateAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var systemAdminUser = await _userRepository.GetSystemAdministratorAsync();

            User dbUser = await _userRepository.FindAsync(user.Id);
            dbUser.Department = user.Department;
            dbUser.LastRosterUpdate = user.LastRosterUpdate;
            dbUser.LastSeen = DateTime.Now;
            dbUser.Mobile = user.Mobile;
            dbUser.Name = user.Name;
            dbUser.Nickname = user.Nickname;
            dbUser.Phone = user.Phone;
            dbUser.ReauthenticateUser = false;
            dbUser.SupervisorId = user.SupervisorId;
            dbUser.Title = user.Title;
            dbUser.UpdatedAt = DateTime.Now;
            dbUser.UpdatedBy = systemAdminUser.Id;

            _userRepository.Update(dbUser);
            await _userRepository.SaveAsync();
        }

        /// <summary>
        /// Perform necessary housekeeping and then mark a user as deleted/disabled.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="asOf">Informational date and time when they were marekd disabled</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "User delete housekeeping shouldn't prevent the user deletion")]
        public async Task MarkUserDisabledAsync(string username, DateTime asOf)
        {
            try
            {
                var user = await _userRepository.FindByUsernameAsync(username);
                var supervisorUser = await _userRepository.GetSupervisorAsync(user.Id);
                while (supervisorUser?.IsDeleted == true)
                {
                    if (!supervisorUser.SupervisorId.HasValue)
                    {
                        throw new OcudaException($"Could find no active users above user {username}");
                    }
                    supervisorUser = await _userRepository.GetSupervisorAsync(supervisorUser.Id);
                }

                // remap any volunteer assignments
                var volunteerMappings = await _volunteerFormService.GetUserMappingsAsync(user.Id);
                foreach (var mapping in volunteerMappings)
                {
                    try
                    {
                        var form = await _volunteerFormService
                            .GetFormByIdAsync(mapping.VolunteerFormId);
                        if (supervisorUser != null)
                        {
                            await _volunteerFormService.AddFormUserMapping(mapping.LocationId,
                                form.VolunteerFormType,
                                supervisorUser.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Unable to reassign {FormType} to a supervisor - no supervisor found for {Username}",
                                form.VolunteerFormType,
                                username);
                        }
                        await _volunteerFormService.RemoveFormUserMapping(mapping.LocationId,
                            user.Id,
                            form.VolunteerFormType);
                        _logger.LogInformation("Reassigned volunteer form type {FormType} to go to supervisor {SupervisorUsername} of disabled user {Username}",
                            form.VolunteerFormType,
                            supervisorUser.Username,
                            username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Couldn't reassign volunteer form id {FormId} to supervisor {SupervisorUsername} while disabling username {Username}: {ErrorMessage}",
                            mapping.VolunteerFormId,
                            supervisorUser?.Username,
                            username,
                            ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Couldn't reassign volunteer forms while disabling username {Username}: {ErrorMessage}",
                    username,
                    ex.Message);
            }

            await _userRepository.MarkUserDeletedAsync(username, GetCurrentUserId(), asOf);
        }

        public async Task RemoveProfilePictureAsync(int userId)
        {
            var user = await _userRepository.FindAsync(userId)
                ?? throw new OcudaException($"Cannot find user ID {userId}");

            var fullPath = GetProfilePictureFilePath(user.PictureFilename);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            user.PictureFilename = null;
            user.PictureUpdatedBy = GetCurrentUserId();

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsUserProfilePicture,
                user.Username);

            await _cache.RemoveAsync(cacheKey);
        }

        public async Task UnsetManualLocationAsync(int userId)
        {
            var user = await _userRepository.FindAsync(userId)
                ?? throw new OcudaException($"Cannot find user id {userId}");

            user.AssociatedLocationManuallySet = false;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }

        public async Task UpdateLocationAsync(int userId, int locationId)
        {
            if (userId != GetCurrentUserId() && !IsSiteManager())
            {
                throw new OcudaException("Permission denied.");
            }

            var user = await _userRepository.FindAsync(userId)
                ?? throw new OcudaException($"Cannot find user id {userId}");
            user.AssociatedLocation = locationId;
            user.AssociatedLocationManuallySet = true;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }

        public async Task<User> UpdateRosterUserAsync(int rosterUserId, User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var systemAdminUser = await _userRepository.GetSystemAdministratorAsync();

            User rosterUser = await _userRepository.FindAsync(rosterUserId);
            rosterUser.Department = user.Department;
            rosterUser.Email = user.Email;
            rosterUser.Mobile = user.Mobile;
            rosterUser.Name = user.Name;
            rosterUser.Nickname = user.Nickname;
            rosterUser.Phone = user.Phone;
            rosterUser.UpdatedAt = DateTime.Now;
            rosterUser.UpdatedBy = systemAdminUser.Id;
            rosterUser.Username = user.Username;

            _userRepository.Update(rosterUser);
            await _userRepository.SaveAsync();

            return rosterUser;
        }

        public async Task UploadProfilePictureAsync(User user, string profilePictureBase64)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            var (extension, profilePicture) = _imageService.ConvertFromBase64(profilePictureBase64, true);

            var checkPath = GetProfilePictureFilePath(null);
            if (!System.IO.Directory.Exists(checkPath))
            {
                System.IO.Directory.CreateDirectory(checkPath);
            }

            var filename = FileHelper.MakeValidFilename(
                System.IO.Path.ChangeExtension(user.Username, extension));

            var fullPath = GetProfilePictureFilePath(filename);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            await System.IO.File.WriteAllBytesAsync(fullPath, profilePicture);

            user.PictureFilename = filename;
            user.PictureUpdatedBy = GetCurrentUserId();

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsUserProfilePicture,
                user.Username);

            await _cache.RemoveAsync(cacheKey);
        }

        private string GetProfilePictureFilePath(string filename)
        {
            return _pathResolver.GetPrivateContentFilePath(filename, ProfilePicturePath);
        }
    }
}