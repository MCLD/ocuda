using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;

        public UserService(ILogger<UserService> logger,
            IUserRepository userRepository)
        {
            _logger = logger
               ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository 
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User> LookupUser(string username)
        {
            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task<User> AddUser(User user, int? createdById = null)
        {
            user.Username = user.Username.Trim();
            user.CreatedAt = DateTime.Now;
            if(createdById != null)
            {
                user.CreatedBy = (int)createdById;
            }

            await ValidateUser(user);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();
            if (createdById != null)
            {
                return user;
            }
            {
                var createdUser = await _userRepository.FindByUsernameAsync(user.Username);
                createdUser.CreatedBy = createdUser.Id;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
                return createdUser;
            }
        }

        /// <summary>
        /// Ensure the sysadmin user exists.
        /// </summary>
        public async Task<User> EnsureSysadminUserAsync()
        {
            var sysadminUser = await _userRepository.GetSystemAdministratorAsync();
            if (sysadminUser == null)
            {
                sysadminUser = new User
                {
                    Username = "sysadmin",
                    CreatedAt = DateTime.Now,
                    IsSysadmin = true
                };
                await _userRepository.AddAsync(sysadminUser);
                await _userRepository.SaveAsync();
            }
            return sysadminUser;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _userRepository.FindAsync(id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var message = string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                message = $"Username name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task<User> EditNicknameAsync(User user)
        {
            var currentUser = await _userRepository.FindAsync(user.Id);
            currentUser.Nickname = user.Nickname;

            await ValidateUser(currentUser);

            _userRepository.Update(currentUser);
            await _userRepository.SaveAsync();
            return currentUser;
        }

        public async Task LoggedInUpdateAsync(User user)
        {
            var dbUser = await GetByIdAsync(user.Id);
            dbUser.LastRosterUpdate = user.LastRosterUpdate;
            dbUser.LastSeen = DateTime.Now;
            dbUser.ReauthenticateUser = false;
            dbUser.Name = user.Name;
            dbUser.Nickname = user.Nickname;
            dbUser.Title = user.Title;
            dbUser.Phone = user.Phone;
            dbUser.SupervisorId = user.SupervisorId;
            _userRepository.Update(dbUser);
            await _userRepository.SaveAsync();
        }

        public async Task<bool> UsernameExistsAsync(User user)
        {
            var existingUser = await GetByUsernameAsync(user.Username);

            if (existingUser != null)
            {
                return existingUser.Id != user.Id ? true : false;
            }

            return false;
        }

        public async Task ValidateUser(User user)
        {
            var message = string.Empty;

            if(string.IsNullOrWhiteSpace(user.Username))
            {
                message = $"Username name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (await UsernameExistsAsync(user))
            {
                message = $"User '{user.Username}' already exists.";
                _logger.LogWarning(message, user.Username);
                throw new OcudaException(message);
            }
        }
    }
}
