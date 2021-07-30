using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFileService
    {
        Task<FileLibrary> CreateLibraryAsync(FileLibrary library, int sectionId);

        Task<File> CreatePrivateFileAsync(File file, IFormFile fileDatas);

        Task<File> CreatePublicFileAsync(File file, IFormFile fileData);

        Task DeleteFileTypesByLibrary(int liAbid);

        Task DeleteLibraryAsync(int id);

        Task DeletePrivateFileAsync(int id);

        Task DeletePublicFileAsync(int id);

        Task<FileLibrary> EditLibraryTypesAsync(FileLibrary library, ICollection<int> fileTypeIds);

        Task<File> EditPrivateFileAsync(File file, IFormFile fileData,
            ICollection<IFormFile> thumbnailFiles, int[] thumbnailIdsToKeep);

        Task<ICollection<int>> GetAllFileTypeIdsAsync();

        Task<ICollection<FileType>> GetAllFileTypesAsync();

        Task<File> GetByIdAsync(int id);

        Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId);

        Task<ICollection<FileType>> GetFileLibrariesFileTypesAsync(int libraryId);

        Task<ICollection<File>> GetFileLibraryFilesAsync(int id);

        Task<FileType> GetFileTypeByIdAsync(int id);

        Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId);

        Task<FileLibrary> GetLibraryByIdAsync(int id);

        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);

        Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter);

        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter);

        string GetPrivateFilePath(File file);

        string GetPublicFilePath(File file);

        Task<byte[]> ReadPrivateFileAsync(File file);

        Task UpdateLibrary(FileLibrary library);
    }
}