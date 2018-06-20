using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IUserRepository : IRepository<User, int>
    {
        Models.User GetSystemAdministrator();
    }
}
