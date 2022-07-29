using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class UserRepository
        : OpsRepository<OpsContext, User, int>, IUserRepository
    {
        public UserRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<UserRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public override async Task<User> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Email == email && !_.IsSysadmin)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByUsernameAsync(string username)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Username == username && !_.IsSysadmin)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindIncludeDeletedAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && !_.IsSysadmin)
                .ToListAsync();
        }

        public async Task<ICollection<User>> GetDirectReportsAsync(int userId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SupervisorId == userId)
                .Select(_ => new User
                {
                    Name = _.Name,
                    Username = _.Username
                })
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<User> GetNameUsernameAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .Select(_ => new User
                {
                    Id = _.Id,
                    IsDeleted = _.IsDeleted,
                    Name = _.Name,
                    Username = _.Username
                })
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetSupervisorAsync(int userId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == userId)
                .Select(_ => _.Supervisor)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateEmail(User user)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Email == user.Email
                         && _.Id != user.Id
                         && !_.IsDeleted
                         && !_.IsSysadmin)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicateUsername(User user)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Username == user.Username
                         && _.Id != user.Id
                         && !_.IsDeleted
                         && !_.IsSysadmin)
                .AnyAsync();
        }

        public async Task<CollectionWithCount<User>> SearchAsync(SearchFilter searchFilter)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.IsInLatestRoster == true);

            if (!string.IsNullOrEmpty(searchFilter.SearchText))
            {
                query = query.Where(_ => _.Name.Contains(searchFilter.SearchText)
                || _.Email.Contains(searchFilter.SearchText)
                || _.Username.Contains(searchFilter.SearchText));
            }
            return new CollectionWithCount<User>
            {
                Count = await query.CountAsync(),
                Data = await query.OrderBy(_ => _.Name).ApplyPagination(searchFilter).ToListAsync()
            };
        }

        public async Task<IEnumerable<int>> SearchIdsAsync(SearchFilter searchFilter)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && (
                        _.Name.Contains(searchFilter.SearchText)
                        || _.Email.Contains(searchFilter.SearchText)
                        || _.Username.Contains(searchFilter.SearchText)))
                .Select(_ => _.Id)
                .ToListAsync();
        }

        #region Initial setup methods

        public async Task<User> GetNonSupervisorAsync(int locationId)
        {
            var supervisorIds = DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && !_.IsSysadmin && _.SupervisorId != null)
                .Select(_ => _.SupervisorId)
                .Distinct();

            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted
                   && !_.IsSysadmin
                   && _.AssociatedLocation == locationId
                   && !supervisorIds.Contains(_.Id))
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetProfilePictureFilenameAsync(string username)
        {
            return await DbSet
                 .AsNoTracking()
                 .Where(_ => _.Username == username)
                 .Select(_ => _.PictureFilename)
                 .SingleOrDefaultAsync();
        }

        public async Task<User> GetSystemAdministratorAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsSysadmin)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<string>> GetTitlesAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !string.IsNullOrEmpty(_.Title))
                .OrderBy(_ => _.Title)
                .Select(_ => _.Title)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> IsSupervisor(int userId)
        {
            return await DbSet
                .AsNoTracking()
                .AnyAsync(_ => _.SupervisorId == userId);
        }

        #endregion Initial setup methods
    }
}
