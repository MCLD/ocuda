using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        Task<User> GetSystemAdministratorAsync();
        Task<User> FindByUsernameAsync(string username);
        Task<bool> IsDuplicateUsername(string username);
        Task<bool> IsDuplicateEmail(string email);
    }
}
