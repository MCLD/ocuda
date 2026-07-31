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
        Task<File> AddFileLibraryFileAsync(File file, IFormFile fileDatas);

        Task AddThumbnailAsync(int fileId, string thumbnailFile);

        Task<FileLibrary> CreateLibraryAsync(Section section, FileLibrary library);

        Task DeleteFileLibraryAsync(Section section, int fileLibraryId);

        Task DeleteFileLibraryFileAsync(string section, string fileLibrarySlug, File file);

        Task DeleteFileTypesByLibrary(int fileLibraryId);

        Task DeleteThumbnailAsync(int thumbnailId);

        Task EditFileLibraryFileAsync(string sectionSlug, string fileLibrarySlug, File file);

        Task<FileLibrary> EditLibraryTypesAsync(FileLibrary library, ICollection<int> fileTypeIds);

        Task<ICollection<FileType>> GetAllFileTypesAsync();

        Task<File> GetByIdAsync(int id);

        Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId);

        Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId, bool? isFeatured);

        Task<FileLibrary> GetBySectionIdSlugAsync(int sectionId, string slug);

        Task<int> GetFileCountAsync(int fileLibraryId);

        Task<ICollection<FileType>> GetFileLibrariesFileTypesAsync(int libraryId);

        Task<File> GetFileLibraryFileAsync(int fileId);

        string GetFileLibraryFilePath(string sectionSlug, string fileLibrarySlug, File file);

        Task<FileType> GetFileTypeByIdAsync(int id);

        Task<FileLibrary> GetLibraryByIdAsync(int id);

        Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId);

        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            Section section,
            FilesFilter filter);

        string GetPrivateFilePath(File file);

        string GetPublicFilePath(File file);

        Task<string> GetThumbnailPathAsync(
            int thumbnailId,
            string fileLibrarySlug,
            string sectionSlug);

        Task<IEnumerable<FileThumbnail>> GetThumbnailsAsync(IEnumerable<int> fileIds);

        string GetUnusedThumbnailPath(string filename, string fileLibrarySlug, string sectionSlug);

        Task<bool> HasReplaceRightsAsync(int fileLibraryId);

        Task<byte[]> ReadPrivateFileAsync(File file);

        Task<File> ReplaceFileLibraryFileAsync(int fileId);

        Task UpdateLibrary(Section section, string fileLibrarySlug, FileLibrary library);

        Task<string> VerifyAddFileAsync(
            Section section,
            int fileLibraryId,
            string extension,
            string filename);
    }
}
