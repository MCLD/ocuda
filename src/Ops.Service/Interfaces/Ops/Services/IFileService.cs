using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        Task<DataWithCount<ICollection<File>>> GetPaginatedGalleryListAsync(BlogFilter filter);
        Task<File> CreatePrivateFileAsync(int currentUserId, 
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles);
        Task<File> EditPrivateFileAsync(int currentUserId, 
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles, int[] thumbnailIds);
        string GetSharedFilePath(File file);
        string GetPrivateFilePath(File file);
        Task DeletePrivateFileAsync(int id);
        Task<byte[]> ReadPrivateFileAsync(File file);
    }
}
