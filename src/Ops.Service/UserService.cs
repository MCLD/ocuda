using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Ocuda.Ops.Service
{
    public class UserService : BaseService<UserService>, IUserService
    {
        private const string ProfilePicturePath = "profilepicture";
        private static readonly string[] ProfilePictureValidTypes = { ".jpg", ".png" };

        private readonly IOcudaCache _cache;
        private readonly IPathResolverService _pathResolver;
        private readonly ITitleClassService _titleClassService;
        private readonly IUserRepository _userRepository;

        public UserService(ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor,
            IOcudaCache cache,
            IPathResolverService pathResolver,
            ITitleClassService titleClassService,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _titleClassService = titleClassService
                ?? throw new ArgumentNullException(nameof(titleClassService));
            _userRepository = userRepository
                    ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User> AddUser(User user, int? createdById = null)
        {
            user.Username = user.Username?.Trim().ToLower();
            user.Email = user.Email?.Trim().ToLower();
            user.CreatedAt = DateTime.Now;
            if (createdById != null)
            {
                user.CreatedBy = (int)createdById;
            }

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();
            if (createdById != null)
            {
                return user;
            }
            {
                User createdUser = await _userRepository.FindByUsernameAsync(user.Username);
                createdUser.CreatedBy = createdUser.Id;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
                return createdUser;
            }
        }

        public async Task<User> EditNicknameAsync(User user)
        {
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
            return sysadminUser;
        }

        public async Task<CollectionWithCount<User>> FindAsync(SearchFilter filter)
        {
            return await _userRepository.SearchAsync(filter);
        }

        public async Task<IEnumerable<int>> FindIdsAsync(SearchFilter filter)
        {
            return await _userRepository.SearchIdsAsync(filter);
        }

        public async Task<int?> GetAssociatedLocation(int userId)
        {
            var user = await _userRepository.FindAsync(userId);
            return user?.AssociatedLocation;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _userRepository.FindAsync(id);
        }

        public async Task<User> GetByIdIncludeDeletedAsync(int id)
        {
            return await _userRepository.FindIncludeDeletedAsync(id);
        }

        public async Task<ICollection<User>> GetDirectReportsAsync(int userId)
        {
            return await _userRepository.GetDirectReportsAsync(userId);
        }

        public async Task<User> GetNameUsernameAsync(int id)
        {
            // TODO add caching
            return await _userRepository.GetNameUsernameAsync(id);
        }

        public async Task<FileDownload> GetProfilePictureAsync(string username)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsUserProfilePicture,
                username);

            var filename = await _cache.GetStringFromCache(cacheKey);

            if (string.IsNullOrEmpty(filename))
            {
                filename = await _userRepository.GetProfilePictureFilenameAsync(username);
                await _cache.SaveToCacheAsync(cacheKey, filename, 8);
            }

            var filePath = GetProfilePictureFilePath(filename);

            new FileExtensionContentTypeProvider()
                .TryGetContentType(filePath, out string fileType);

            return new FileDownload
            {
                Filename = filename,
                FileType = fileType,
                FileData = await System.IO.File.ReadAllBytesAsync(filePath)
            };
        }

        public async Task<IDictionary<TitleClass, ICollection<User>>>
                    GetRelatedTitleClassificationsAsync(int userId)

        {
            var result = new Dictionary<TitleClass, ICollection<User>>();
            var supervisor = await GetSupervisorAsync(userId);
            while (supervisor != null)
            {
                var titles = await _titleClassService.GetTitleClassByTitleAsync(supervisor.Title);
                foreach (var title in titles)
                {
                    if (!result.ContainsKey(title))
                    {
                        result[title] = new List<User>();
                    }
                    result[title].Add(new User
                    {
                        Email = supervisor.Email,
                        Id = supervisor.Id,
                        Name = supervisor.Name,
                        Username = supervisor.Username
                    });
                }
                supervisor = await GetSupervisorAsync(supervisor.Id);
            }
            return result;
        }

        public async Task<User> GetSupervisorAsync(int userId)
        {
            return await _userRepository.GetSupervisorAsync(userId);
        }

        public async Task<ICollection<string>> GetTitlesAsync()
        {
            // TODO add caching
            return await _userRepository.GetTitlesAsync();
        }

        public async Task<bool> IsSupervisor(int userId)
        {
            // TODO add caching
            return await _userRepository.IsSupervisor(userId);
        }

        public async Task LoggedInUpdateAsync(User user)
        {
            var systemAdminUser = await _userRepository.GetSystemAdministratorAsync();

            User dbUser = await GetByIdAsync(user.Id);
            dbUser.LastRosterUpdate = user.LastRosterUpdate;
            dbUser.LastSeen = DateTime.Now;
            dbUser.ReauthenticateUser = false;
            dbUser.Name = user.Name;
            dbUser.Nickname = user.Nickname;
            dbUser.Title = user.Title;
            dbUser.Phone = user.Phone;
            dbUser.SupervisorId = user.SupervisorId;
            dbUser.UpdatedAt = DateTime.Now;
            dbUser.UpdatedBy = systemAdminUser.Id;

            _userRepository.Update(dbUser);
            await _userRepository.SaveAsync();
        }

        public async Task<User> LookupUserAsync(string username)
        {
            return await _userRepository.FindByUsernameAsync(username?.Trim().ToLower());
        }

        public async Task<User> LookupUserByEmailAsync(string email)
        {
            return await _userRepository.FindByEmailAsync(email?.Trim().ToLower());
        }

        public async Task RemoveProfilePictureAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
            {
                throw new OcudaException($"Cannot find user ID {userId}");
            }

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

        public async Task UpdateLocationAsync(int userId, int locationId)
        {
            if (userId != GetCurrentUserId() && !IsSiteManager())
            {
                throw new OcudaException("Permission denied.");
            }

            var user = await _userRepository.FindAsync(userId);
            if (user == null)
            {
                throw new OcudaException($"Cannot find user id {userId}");
            }
            user.AssociatedLocation = locationId;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }

        public async Task<User> UpdateRosterUserAsync(int rosterUserId, User user)
        {
            var systemAdminUser = await _userRepository.GetSystemAdministratorAsync();

            User rosterUser = await GetByIdAsync(rosterUserId);
            rosterUser.Email = user.Email;
            rosterUser.Name = user.Name;
            rosterUser.Nickname = user.Nickname;
            rosterUser.Phone = user.Phone;
            rosterUser.Username = user.Username;
            rosterUser.UpdatedAt = DateTime.Now;
            rosterUser.UpdatedBy = systemAdminUser.Id;

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

            byte[] profilePicture;

            string extension;
            try
            {
                profilePicture = Convert.FromBase64String(profilePictureBase64);

                var imageInfo = Image.Identify(profilePicture, out IImageFormat imageFormat);
                if (imageInfo.Height != imageInfo.Width)
                {
                    throw new OcudaException("Profile picture must be square.");
                }

                var validFileType = false;
                foreach (var validExtension in ProfilePictureValidTypes)
                {
                    if (imageFormat.MimeTypes.Contains(MimeTypes.GetMimeType(validExtension)))
                    {
                        validFileType = true;
                        break;
                    }
                }

                if (!validFileType)
                {
                    throw new OcudaException("Invalid image format, please upload a JPEG or PNG picture");
                }
                extension = imageFormat.FileExtensions.First();
            }
            catch (UnknownImageFormatException uifex)
            {
                throw new OcudaException("Unknown image type, please upload a JPEG or PNG picture",
                    uifex);
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
