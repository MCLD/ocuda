﻿using System;
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

        public async Task<User> FindByUsernameAsync(string username)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Username == username)
                .FirstOrDefaultAsync();
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
        // this cannot be async becuase Configure() in Startup.cs is not async
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
