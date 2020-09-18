﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category> FindAsync(int id);

        Task<ICollection<Category>> GetAllAsync();

        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter);

        Category GetByClass(string categoryClass);
    }
}
