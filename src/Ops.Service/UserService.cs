using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class UserService : BaseService<UserService>, IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User> LookupUserAsync(string username)
        {
            return await _userRepository.FindByUsernameAsync(username?.Trim().ToLower());
        }

        public async Task<User> LookupUserByEmailAsync(string email)
        {
            return await _userRepository.FindByEmailAsync(email?.Trim().ToLower());
        }

        public async Task<Tuple<string, string>> GetUserInfoById(int id)
        {
            return await _userRepository.GetUserInfoById(id);
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

        public async Task LoggedInUpdateAsync(User user)
        {
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
            dbUser.UpdatedBy = GetCurrentUserId();

            _userRepository.Update(dbUser);
            await _userRepository.SaveAsync();
        }

        public async Task<User> UpdateRosterUserAsync(int rosterUserId, User user)
        {
            User rosterUser = await GetByIdAsync(rosterUserId);
            rosterUser.Email = user.Email;
            rosterUser.Name = user.Name;
            rosterUser.Nickname = user.Nickname;
            rosterUser.Phone = user.Phone;
            rosterUser.Username = user.Username;
            rosterUser.UpdatedAt = DateTime.Now;
            rosterUser.UpdatedBy = GetCurrentUserId();

            _userRepository.Update(rosterUser);
            await _userRepository.SaveAsync();

            return rosterUser;
        }

        public async Task<ICollection<User>> GetDirectReportsAsync(int supervisorId)
        {
            return await _userRepository.GetDirectReportsAsync(supervisorId);
        }
    }
}
