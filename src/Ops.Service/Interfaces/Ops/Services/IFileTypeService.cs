using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFileTypeService
    {
        Task<ICollection<FileType>> GetAllAsync();
        Task<ICollection<FileType>> GetAllExtensionsAsync();
        Task<FileType> GetByExtensionAsync(string extension);
        Task<int> GetIdByExtensionAsync(string extension);
        Task<FileType> GetByIdAsync(int id);
    }
}
