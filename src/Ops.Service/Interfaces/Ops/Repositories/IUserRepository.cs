using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserRepository : IOpsRepository<User, int>
    {
        Task<User> FindByEmailAsync(string email);

        Task<User> FindByUsernameAsync(string username);

        Task<ICollection<User>> GetAllAsync();

        Task<ICollection<User>> GetDirectReportsAsync(int userId);

        Task<(string name, string username)> GetNameUsernameAsync(int id);

        Task<User> GetSystemAdministratorAsync();

        Task<bool> IsDuplicateEmail(User user);

        Task<bool> IsDuplicateUsername(User user);

        Task<bool> IsSupervisor(int userId);

        Task<CollectionWithCount<User>> SearchAsync(SearchFilter searchFilter);
    }
}
