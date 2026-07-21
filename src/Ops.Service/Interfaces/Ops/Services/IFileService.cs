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
        string GetUnusedThumbnailPath(string filename, string fileLibrarySlug, string sectionSlug);

        Task<IEnumerable<FileThumbnail>> GetThumbnailsAsync(IEnumerable<int> fileIds);

        Task AddThumbnailAsync(int fileId, string thumbnailFile);

        Task DeleteThumbnailAsync(int thumbnailId);

        Task<File> AddFileLibraryFileAsync(File file, IFormFile fileDatas);

        Task<FileLibrary> CreateLibraryAsync(FileLibrary library);

        Task DeleteFileTypesByLibrary(int fileLibraryId);

        Task DeleteLibraryAsync(int sectionId, int fileLibraryId);

        Task DeletePrivateFileAsync(int sectionId, string fileLibrarySlug, int fileId);

        Task<FileLibrary> EditLibraryTypesAsync(FileLibrary library, ICollection<int> fileTypeIds);

        Task<ICollection<FileType>> GetAllFileTypesAsync();

        Task<File> GetByIdAsync(int id);

        Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId);

        Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId, bool? isFeatured);

        Task<FileLibrary> GetBySectionIdSlugAsync(int sectionId, string slug);

        Task<ICollection<FileType>> GetFileLibrariesFileTypesAsync(int libraryId);

        Task<string> GetFilePathAsync(int sectionId, string librarySlug, int fileId);

        Task<string> GetThumbnailPathAsync(
            int thumbnailId,
            string fileLibrarySlug,
            string sectionSlug);

        Task<FileType> GetFileTypeByIdAsync(int id);

        Task<FileLibrary> GetLibraryByIdAsync(int id);

        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);

        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(FilesFilter filter);

        Task<int> GetFileCountAsync(int fileLibraryId);

        string GetPrivateFilePath(File file);

        string GetPublicFilePath(File file);

        Task<bool> HasReplaceRightsAsync(int fileLibraryId);

        Task<byte[]> ReadPrivateFileAsync(File file);

        Task<File> ReplaceFileLibraryFileAsync(int fileId);

        Task UpdateLibrary(string sectionSlug, string fileLibrarySlug, FileLibrary library);

        Task<string> VerifyAddFileAsync(int fileLibraryId, string extension, string filename);
    }
}
