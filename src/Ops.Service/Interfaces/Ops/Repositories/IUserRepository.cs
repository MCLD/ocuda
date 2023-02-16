using System;
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

        Task<User> FindIncludeDeletedAsync(int id);

        Task<User> FindUsernameIncludeDeletedAsync(string username);

        Task<ICollection<User>> GetAllAsync();

        Task<ICollection<User>> GetDirectReportsAsync(int userId);

        Task<User> GetNameUsernameAsync(int id);

        Task<string> GetProfilePictureFilenameAsync(string username);

        Task<User> GetSupervisorAsync(int userId);

        Task<User> GetSystemAdministratorAsync();

        Task<ICollection<string>> GetTitlesAsync();

        Task<bool> IsDuplicateEmail(User user);

        Task<bool> IsDuplicateUsername(User user);

        Task<bool> IsSupervisor(int userId);

        Task MarkUserDeletedAsync(string username, int currentUserId, DateTime asOf);

        Task<CollectionWithCount<User>> SearchAsync(SearchFilter searchFilter);

        Task<IEnumerable<int>> SearchIdsAsync(SearchFilter searchFilter);

        Task UpdateSupervisor(int userId, int supervisorId);
    }
}