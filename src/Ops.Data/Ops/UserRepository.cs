using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class UserRepository
        : GenericRepository<User, int>, IUserRepository
    {
        public UserRepository(OpsContext context, ILogger<UserRepository> logger)
            : base(context, logger)
        {
        }

        public override async Task<User> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDeleted == false && _.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByUsernameAsync(string username)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDeleted == false && _.Username == username && _.IsSysadmin == false)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDeleted == false && _.Email == email && _.IsSysadmin == false)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDeleted == false && _.IsSysadmin == false)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateUsername(User user)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Username == user.Username
                         && _.Id != user.Id
                         && _.IsDeleted == false
                         && _.IsSysadmin == false)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicateEmail(User user)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Email == user.Email
                         && _.Id != user.Id
                         && _.IsDeleted == false
                         && _.IsSysadmin == false)
                .AnyAsync();
        }

        public async Task<Tuple<string, string>> GetUserInfoById(int id)
        {
            User user = await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();

            return new Tuple<string, string>(user.Name, user.Username);
        }

        public async Task<ICollection<User>> GetDirectReportsAsync(int supervisorId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SupervisorId == supervisorId)
                .Select(_ => new User
                {
                    Name = _.Name,
                    Username = _.Username
                })
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        #region Initial setup methods
        public async Task<User> GetSystemAdministratorAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsSysadmin == true)
                .FirstOrDefaultAsync();
        }
        #endregion Initial setup methods
    }
}
