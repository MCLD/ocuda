using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFileTypeService
    {
        Task<ICollection<FileType>> GetAllAsync();
        Task<ICollection<int>> GetAllIdsAsync();
        Task<ICollection<FileType>> GetAllExtensionsAsync();
        Task<FileType> GetByExtensionAsync(string extension);
        Task<FileType> GetByIdAsync(int id);
        Task<ICollection<FileType>> GetTypesByLibraryIdsAsync(int libId);
    }
}
