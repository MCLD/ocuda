using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFileTypeRepository : IRepository<FileType, int>
    {
        Task<ICollection<FileType>> GetAllExtensionsAsync();
        Task<FileType> GetByExtensionAsync(string extension);
        Task<ICollection<int>> GetAllIdsAsync();
        Task<ICollection<FileType>> GetAllTypesByLibraryIdAsync(int libId);
    }
}
