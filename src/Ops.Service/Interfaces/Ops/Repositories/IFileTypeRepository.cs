using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileTypeRepository : IRepository<FileType, int>
    {
        Task<FileType> GetByExtensionAsync(string extension);
    }
}
