using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserRepository : IOpsRepository<User, int>
    {
        Task<User> FindByEmailAsync(string email);

        Task<User> FindByUsernameAsync(string username);

        Task<ICollection<User>> GetAllAsync();

        Task<ICollection<User>> GetDirectReportsAsync(int supervisorId);

        Task<(string name, string username)> GetNameUsernameAsync(int id);

        Task<User> GetSystemAdministratorAsync();

        Task<bool> IsDuplicateEmail(User user);

        Task<bool> IsDuplicateUsername(User user);
    }
}