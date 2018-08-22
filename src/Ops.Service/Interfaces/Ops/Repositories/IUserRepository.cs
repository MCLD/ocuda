using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        Task<User> GetSystemAdministratorAsync();
        Task<User> FindByUsernameAsync(string username);
        Task<User> FindByEmailAsync(string email);
        Task<ICollection<User>> GetAllAsync();
        Task<bool> IsDuplicateUsername(User user);
        Task<bool> IsDuplicateEmail(User user);
        Task<Tuple<string, string>> GetUserInfoById(int id);
        Task<ICollection<User>> GetDirectReportsAsync(int supervisorId);
    }
}
