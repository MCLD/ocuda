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
    public class UserManagementService(ILogger<UserManagementService> logger,
        IHttpContextAccessor httpContextAccessor,
        IOcudaCache cache,
        IPathResolverService pathResolver,
        IUserRepository userRepository,
        IVolunteerFormService volunteerFormService,
        IImageService imageService)
        : BaseService<UserManagementService>(logger, httpContextAccessor), IUserManagementService
    {
        public static readonly string ProfilePicturePath = "profilepicture";

        public async Task<User> AddUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            user.Username = user.Username?.Trim().ToLowerInvariant();
            user.Email = user.Email?.Trim().ToLowerInvariant();
            user.CreatedAt = DateTime.Now;

            await userRepository.AddAsync(user);
            await userRepository.SaveAsync();

            User createdUser = await userRepository.FindByUsernameAsync(user.Username);
            createdUser.CreatedBy = createdUser.Id;
            userRepository.Update(user);
            await userRepository.SaveAsync();
            return createdUser;
        }

        public async Task<User> EditNicknameAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            User currentUser = await userRepository.FindAsync(user.Id);
            currentUser.Nickname = user.Nickname;
            currentUser.UpdatedAt = DateTime.Now;
            currentUser.UpdatedBy = GetCurrentUserId();

            userRepository.Update(currentUser);
            await userRepository.SaveAsync();
            return currentUser;
        }

        /// <summary>
        /// Ensure the sysadmin user exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<User> EnsureSysadminUserAsync()
        {
            User sysadminUser = await userRepository.GetSystemAdministratorAsync();
            if (sysadminUser == null)
            {
                sysadminUser = new User
                {
                    Username = "sysadmin",
                    Name = "System",
                    CreatedAt = DateTime.Now,
                    IsSysadmin = true,
                };
                await userRepository.AddAsync(sysadminUser);
                await userRepository.SaveAsync();
            }

            if (!sysadminUser.ExcludeFromRoster)
            {
                sysadminUser.ExcludeFromRoster = true;
                userRepository.Update(sysadminUser);
                await userRepository.SaveAsync();
            }

            return sysadminUser;
        }

        public async Task LoggedInUpdateAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var systemAdminUser = await userRepository.GetSystemAdministratorAsync();

            User dbUser = await userRepository.FindAsync(user.Id);
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

            userRepository.Update(dbUser);
            await userRepository.SaveAsync();
        }

        /// <summary>
        /// Perform necessary housekeeping and then mark a user as deleted/disabled.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="asOf">Informational date and time when they were marekd disabled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task MarkUserDisabledAsync(int userId, string username, DateTime asOf)
        {
            try
            {
                var user = await userRepository.FindByUsernameAsync(username);
                var supervisorUser = await userRepository.GetSupervisorAsync(user.Id);
                while (supervisorUser?.IsDeleted == true)
                {
                    if (!supervisorUser.SupervisorId.HasValue)
                    {
                        throw new OcudaException($"Could find no active users above user {username}");
                    }

                    supervisorUser = await userRepository.GetSupervisorAsync(supervisorUser.Id);
                }

                // remap any volunteer assignments
                var volunteerMappings = await volunteerFormService.GetUserMappingsAsync(user.Id);
                foreach (var mapping in volunteerMappings)
                {
                    try
                    {
                        var form = await volunteerFormService
                            .GetFormByIdAsync(mapping.VolunteerFormId);
                        if (supervisorUser != null)
                        {
                            await volunteerFormService.AddFormUserMapping(mapping.LocationId,
                                form.VolunteerFormType,
                                supervisorUser.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Unable to reassign {FormType} to a supervisor - no supervisor found for {Username}",
                                form.VolunteerFormType,
                                username);
                        }

                        await volunteerFormService.RemoveFormUserMapping(mapping.LocationId,
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

            await userRepository.MarkUserDeletedAsync(username, userId, asOf);
        }

        public async Task RemoveProfilePictureAsync(int userId)
        {
            var user = await userRepository.FindAsync(userId)
                ?? throw new OcudaException($"Cannot find user ID {userId}");

            var fullPath = GetProfilePictureFilePath(user.PictureFilename);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                _logger.LogInformation("Deleted profile image {Path} for user {User}",
                    fullPath,
                    user.Username);
            }

            user.PictureFilename = null;
            user.PictureUpdatedBy = GetCurrentUserId();

            userRepository.Update(user);
            await userRepository.SaveAsync();

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsUserProfilePicture,
                user.Username);

            await cache.RemoveAsync(cacheKey);
        }

        public async Task UnsetManualLocationAsync(int userId)
        {
            var user = await userRepository.FindAsync(userId)
                ?? throw new OcudaException($"Cannot find user id {userId}");

            user.AssociatedLocationManuallySet = false;
            userRepository.Update(user);
            await userRepository.SaveAsync();
        }

        public async Task UpdateLocationAsync(int userId, int locationId)
        {
            if (userId != GetCurrentUserId() && !IsSiteManager())
            {
                throw new OcudaException("Permission denied.");
            }

            var user = await userRepository.FindAsync(userId)
                ?? throw new OcudaException($"Cannot find user id {userId}");
            user.AssociatedLocation = locationId;
            user.AssociatedLocationManuallySet = true;
            userRepository.Update(user);
            await userRepository.SaveAsync();
        }

        public async Task<User> UpdateRosterUserAsync(int rosterUserId, User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var systemAdminUser = await userRepository.GetSystemAdministratorAsync();

            User rosterUser = await userRepository.FindAsync(rosterUserId);
            rosterUser.Department = user.Department;
            rosterUser.Email = user.Email;
            rosterUser.Mobile = user.Mobile;
            rosterUser.Name = user.Name;
            rosterUser.Nickname = user.Nickname;
            rosterUser.Phone = user.Phone;
            rosterUser.UpdatedAt = DateTime.Now;
            rosterUser.UpdatedBy = systemAdminUser.Id;
            rosterUser.Username = user.Username;

            userRepository.Update(rosterUser);
            await userRepository.SaveAsync();

            return rosterUser;
        }

        public async Task UploadProfilePictureAsync(User user, string profilePictureBase64)
        {
            ArgumentNullException.ThrowIfNull(user);

            string extension;
            byte[] profilePicture;

            try
            {
                (extension, profilePicture) = imageService.ConvertFromBase64(profilePictureBase64, true);
            }
            catch (OcudaException oex)
            {
                _logger.LogError("Error converting profile picture from base64: {ErrorMessage}",
                    oex.Message);
                throw;
            }

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
                _logger.LogInformation("Deleted profile image {Path} for user {User}",
                    fullPath,
                    user.Username);
            }

            await System.IO.File.WriteAllBytesAsync(fullPath, profilePicture);

            user.PictureFilename = filename;
            user.PictureUpdatedBy = GetCurrentUserId();

            userRepository.Update(user);
            await userRepository.SaveAsync();

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsUserProfilePicture,
                user.Username);

            await cache.RemoveAsync(cacheKey);
        }

        private string GetProfilePictureFilePath(string filename)
        {
            return pathResolver.GetPrivateContentFilePath(filename, ProfilePicturePath);
        }
    }
}
