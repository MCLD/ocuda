using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class UserService : BaseService<UserService>, IUserService
    {
        private readonly IOcudaCache _cache;
        private readonly IPathResolverService _pathResolver;
        private readonly ITitleClassService _titleClassService;
        private readonly IUserRepository _userRepository;

        public UserService(IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger,
            IOcudaCache cache,
            IPathResolverService pathResolver,
            ITitleClassService titleClassService,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(pathResolver);
            ArgumentNullException.ThrowIfNull(titleClassService);
            ArgumentNullException.ThrowIfNull(userRepository);

            _cache = cache;
            _pathResolver = pathResolver;
            _titleClassService = titleClassService;
            _userRepository = userRepository;
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

        public async Task<ICollection<User>> GetDirectReportsAsync(int supervisorId)
        {
            return await _userRepository.GetDirectReportsAsync(supervisorId);
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

            var filePath = _pathResolver.GetPrivateContentFilePath(filename,
                UserManagementService.ProfilePicturePath);

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Normalize username to lowercase.")]
        public async Task<User> LookupUserAsync(string username)
        {
            return await _userRepository
                .FindByUsernameAsync(username?.Trim().ToLowerInvariant());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Normalize email address to lowercase.")]
        public async Task<User> LookupUserByEmailAsync(string email)
        {
            return await _userRepository
                .FindByEmailAsync(email?.Trim().ToLowerInvariant());
        }
    }
}