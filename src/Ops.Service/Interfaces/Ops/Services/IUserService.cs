using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IUserService
    {
        Task<User> AddUser(User user, int? createdById = null);

        Task<User> EditNicknameAsync(User user);

        Task<User> EnsureSysadminUserAsync();

        Task<User> GetByIdAsync(int id);

        Task<ICollection<User>> GetDirectReportsAsync(int supervisorId);

        Task<(string name, string username)> GetNameUsernameAsync(int id);

        Task LoggedInUpdateAsync(User user);

        Task<User> LookupUserAsync(string username);

        Task<User> LookupUserByEmailAsync(string email);

        Task<User> UpdateRosterUserAsync(int rosterUserId, User user);
    }
}