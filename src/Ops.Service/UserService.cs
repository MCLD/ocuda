﻿using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Service
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository 
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User> LookupUser(string username)
        {
            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task AddUser(User user, int? createdById = null)
        {
            user.Username = user.Username.Trim();
            user.CreatedAt = DateTime.Now;
            if(createdById != null)
            {
                user.CreatedBy = (int)createdById;
            }
            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();
            if (createdById == null)
            {
                var createdUser = await _userRepository.FindByUsernameAsync(user.Username);
                createdUser.CreatedBy = createdUser.Id;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
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
            // TODO throw exception if the username is null
            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task<User> EditNicknameAsync(User user)
        {
            var currentUser = await _userRepository.FindAsync(user.Id);
            currentUser.Nickname = user.Nickname;

            _userRepository.Update(currentUser);
            await _userRepository.SaveAsync();
            return currentUser;
        }
    }
}
