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
        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync( 
            BlogFilter filter, bool isGallery = false);
        Task<File> CreatePrivateFileAsync(int currentUserId, 
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles);
        Task<File> EditPrivateFileAsync(int currentUserId, 
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles, int[] thumbnailIds);
        Task<File> CreatePublicFileAsync(int currentUserId, File file, IFormFile fileData);
        string GetPublicFilePath(File file);
        string GetPrivateFilePath(File file);
        Task DeletePrivateFileAsync(int id);
        Task<byte[]> ReadPrivateFileAsync(File file);
        Task<IEnumerable<int>> GetFileTypeIdsInUseByCategoryIdAsync(int categoryId);
        Task<IEnumerable<File>> GetByPageIdAsync(int pageId);
        Task<IEnumerable<File>> GetByPostIdAsync(int postId);
        Task DeletePublicFileAsync(int id);
    }
}
