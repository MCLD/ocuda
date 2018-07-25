using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class UserRepository
        : GenericRepository<Models.User, int>, IUserRepository
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
                .Where(_ => _.IsDeleted == false 
                    && string.Equals(_.Username, sanitizedUsername, 
                        StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            var sanitizedEmail = email.Trim();
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDeleted == false 
                    && string.Equals(_.Email, sanitizedEmail,
                        StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDeleted == false && _.IsSysadmin == false)
                .ToListAsync();
        }

        public async Task<bool> IsDuplicateUsername(string username)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Username == username)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicateEmail(string email)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Email == email)
                .AnyAsync();
        }

        #region Initial setup methods
        public async Task<Models.User> GetSystemAdministratorAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsSysadmin == true)
                .FirstOrDefaultAsync();
        }
        #endregion Initial setup methods
    }
}
