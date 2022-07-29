using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IUserService
    {
        Task<User> AddUser(User user, int? createdById = null);

        Task<User> EditNicknameAsync(User user);

        Task<User> EnsureSysadminUserAsync();

        Task<CollectionWithCount<User>> FindAsync(SearchFilter filter);

        Task<IEnumerable<int>> FindIdsAsync(SearchFilter filter);

        Task<int?> GetAssociatedLocation(int userId);

        Task<User> GetByIdAsync(int id);

        Task<User> GetByIdIncludeDeletedAsync(int id);

        Task<ICollection<User>> GetDirectReportsAsync(int supervisorId);

        Task<User> GetNameUsernameAsync(int id);

        Task<FileDownload> GetProfilePictureAsync(string username);

        Task<IDictionary<TitleClass, ICollection<User>>> GetRelatedTitleClassificationsAsync(int userId);

        Task<User> GetSupervisorAsync(int userId);

        Task<ICollection<string>> GetTitlesAsync();

        Task<bool> IsSupervisor(int supervisorId);

        Task LoggedInUpdateAsync(User user);

        Task<User> LookupUserAsync(string username);

        Task<User> LookupUserByEmailAsync(string email);

        Task RemoveProfilePictureAsync(int userId);

        Task UpdateLocationAsync(int userId, int locationId);

        Task<User> UpdateRosterUserAsync(int rosterUserId, User user);

        Task UploadProfilePictureAsync(User user, string profilePictureBase64);
    }
}
