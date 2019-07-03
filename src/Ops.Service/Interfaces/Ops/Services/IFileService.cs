using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFileService
    {
        Task<int> GetFileCountAsync();
        Task<ICollection<File>> GetFilesAsync();
        Task<File> GetByIdAsync(int id);
        Task<File> GetLatestByLibraryIdAsync(int id);
        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter);

        Task<File> CreatePrivateFileAsync(int currentUserId,
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles);

        Task<File> EditPrivateFileAsync(int currentUserId, File file, IFormFile fileData,
            ICollection<IFormFile> thumbnailFiles, int[] thumbnailIdsToKeep);

        Task<File> CreatePublicFileAsync(int currentUserId, File file, IFormFile fileData);
        string GetPublicFilePath(File file);
        string GetPrivateFilePath(File file);
        Task DeletePrivateFileAsync(int id);
        Task<byte[]> ReadPrivateFileAsync(File file);
        Task DeletePublicFileAsync(int id);

        Task<FileLibrary> GetLibraryByIdAsync(int id);

        Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter);

        Task<FileLibrary> CreateLibraryAsync(int currentUserId, FileLibrary library,
            ICollection<int> fileTypeIds);

        Task<FileLibrary> EditLibraryAsync(FileLibrary library, ICollection<int> fileTypeIds);
        Task DeleteLibraryAsync(int id);
        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);
        Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId);
    }
}
