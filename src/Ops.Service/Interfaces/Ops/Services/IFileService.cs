using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFileService
    {
        Task<int> GetFileCountAsync();
        Task<ICollection<File>> GetFilesAsync();
        Task<File> GetByIdAsync(int id);
        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter);
        Task<File> CreatePrivateFileAsync(int currentUserId, File file, byte[] fileData);
        string GetSharedFilePath(File file);
        string GetPrivateFilePath(File file);
        Task<File> EditPrivateFileAsync(File file, byte[] fileData = null);
        Task DeletePrivateFileAsync(int id);
        Task<byte[]> ReadPrivateFileAsync(File file);
    }
}
