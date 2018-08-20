using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICategoryFileTypeRepository : IRepository<CategoryFileType, int>
    {
        Task<IEnumerable<int>> GetFileTypeIdsByCategoryIdAsync(int categoryId);
        void RemoveByCategoryId(int categoryId);
    }
}
