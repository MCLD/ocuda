using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IUserManagementService
    {
        Task<User> AddUser(User user);

        Task<User> EditNicknameAsync(User user);

        Task<User> EnsureSysadminUserAsync();

        Task LoggedInUpdateAsync(User user);

        Task MarkUserDisabledAsync(string username, DateTime asOf);

        Task RemoveProfilePictureAsync(int userId);

        Task UnsetManualLocationAsync(int userId);

        Task UpdateLocationAsync(int userId, int locationId);

        Task<User> UpdateRosterUserAsync(int rosterUserId, User user);

        Task UploadProfilePictureAsync(User user, string profilePictureBase64);
    }
}