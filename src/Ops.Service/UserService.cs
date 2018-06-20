using System;
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

        /// <summary>
        /// Ensure the sysadmin user exists. This is done synchronously so it can be called during
        /// application setup.
        /// </summary>
        public void EnsureSysadminUser()
        {
            var sysadminUser = _userRepository.GetSystemAdministrator();
            if (sysadminUser == null)
            {
                var addTask = _userRepository.AddAsync(new Ocuda.Ops.Models.User
                {
                    Username = "sysadmin",
                    CreatedAt = DateTime.Now,
                    IsSysadmin = true
                });
                addTask.Wait();

                var saveTask = _userRepository.SaveAsync();
                saveTask.Wait();
            }
        }

    }
}
