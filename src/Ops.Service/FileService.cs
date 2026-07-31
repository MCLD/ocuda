using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class FileService(
        ILogger<FileService> logger,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        IFileLibraryRepository fileLibraryRepository,
        IFileRepository fileRepository,
        IFileThumbnailRepository fileThumbnailRepository,
        IFileTypeService fileTypeService,
        IPathResolverService pathResolver,
        IPermissionGroupService permissionGroupService,
        IUserService userService)
        : BaseService<FileService>(logger, httpContextAccessor),
        IFileService
    {
        private const string SectionsPath = "sections";
        private const string ThumbnailsPath = "thumbnails";

        public Task<File> AddFileLibraryFileAsync(File file, IFormFile fileDatas)
        {
            return file == null
                ? throw new ArgumentNullException(nameof(file))
                : fileDatas == null
                    ? throw new ArgumentNullException(nameof(fileDatas))
                    : AddFileLibraryFileInternalAsync(file, fileDatas);
        }

        public async Task AddThumbnailAsync(int fileId, string thumbnailFile)
        {
            await fileThumbnailRepository.AddAsync(new FileThumbnail
            {
                CreatedAt = dateTimeProvider.Now,
                CreatedBy = GetCurrentUserId(),
                FileId = fileId,
                ThumbnailFile = thumbnailFile,
            });
            await fileThumbnailRepository.SaveAsync();
        }

        public async Task<FileLibrary> CreateLibraryAsync(Section section, FileLibrary library)
        {
            ArgumentNullException.ThrowIfNull(section);
            ArgumentNullException.ThrowIfNull(library);

            library.Name = library.Name?.Trim();
            library.Slug = library.Slug?.Trim();

            var exists = await fileLibraryRepository
                .GetBySectionIdSlugAsync(library.SectionId, library.Slug);

            if (exists != null)
            {
                throw new OcudaException(
                    $"A file library for this section already exists with this slug: {library.Slug}");
            }

            var path = pathResolver
                .GetPrivateContentFilePath(null, SectionsPath, section.Slug, library.Slug);

            var directory = new System.IO.DirectoryInfo(path);

            if (directory.GetFiles().Length > 0)
            {
                throw new OcudaException(
                    $"Directory already exists and has {directory.GetFiles().Length} files in it.");
            }

            library.CreatedAt = dateTimeProvider.Now;
            library.CreatedBy = GetCurrentUserId();

            await fileLibraryRepository.AddAsync(library);
            await fileLibraryRepository.SaveAsync();

            return library;
        }

        public async Task DeleteReplacePemissionsAsync(int fileLibraryId)
        {
            var replaceFilePermissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupReplaceFiles>(fileLibraryId);

            foreach (var permission in replaceFilePermissions)
            {
                await permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupReplaceFiles>(
                        permission.FileLibraryId,
                        permission.PermissionGroupId);
            }
        }

        public async Task DeleteFileTypesByLibrary(int fileLibraryId)
        {
            var currentLibrary = await fileLibraryRepository.FindAsync(fileLibraryId);
            var fileTypeIds = await GetLibraryFileTypeIdsAsync(fileLibraryId);
            var fileTypesToRemove = currentLibrary.FileTypes
                .Where(_ => fileTypeIds.Any(__ => __ == _.FileTypeId))
                .Select(_ => _.FileTypeId)
                .ToList();
            await fileLibraryRepository
                .RemoveLibraryFileTypesAsync(fileTypesToRemove, currentLibrary.Id);
        }

        public async Task DeleteFileLibraryAsync(Section section, int fileLibraryId)
        {
            ArgumentNullException.ThrowIfNull(section);

            var fileLibrary = await GetLibraryByIdAsync(fileLibraryId);

            var filePath = pathResolver
                .GetPrivateContentFilePath(null, SectionsPath, section.Slug, fileLibrary.Slug);

            var exists = System.IO.Directory.Exists(filePath);

            if (exists)
            {
                try
                {
                    System.IO.Directory.Delete(filePath, true);
                }
                catch (SystemException ex)
                {
                    throw new OcudaException(
                        $"Unable to delete file library directory: {ex.Message}",
                        ex);
                }
            }

            await DeleteFileTypesByLibrary(fileLibraryId);
            await DeleteReplacePemissionsAsync(fileLibraryId);

            fileLibraryRepository.Remove(fileLibraryId);
            await fileLibraryRepository.SaveAsync();

            if (!exists)
            {
                throw new OcudaException(
                    $"Directory does not exist: {System.IO.Path.GetFileName(filePath)}");
            }
        }

        public async Task DeleteFileLibraryFileAsync(
            string sectionSlug,
            string fileLibrarySlug,
            File file)
        {
            ArgumentNullException.ThrowIfNull(file);

            var lookupFile = await GetFileLibraryFileAsync(file.Id);

            var issues = new StringBuilder();
            try
            {
                var thumbs = await fileThumbnailRepository.GetByFileIdsAsync([lookupFile.Id]);
                if (thumbs?.Count > 0)
                {
                    fileThumbnailRepository.RemoveRange(thumbs);
                    await fileThumbnailRepository.SaveAsync();
                }
            }
            catch (Exception ex) when (ex is SystemException || ex is OcudaException)
            {
                issues.Append("deleting thumbnails: ").Append(ex.Message);
            }

            var filePath = GetFileLibraryFilePath(sectionSlug, fileLibrarySlug, lookupFile);

            var fileExists = System.IO.File.Exists(filePath);

            if (fileExists)
            {
                System.IO.File.Delete(filePath);
            }

            fileRepository.Remove(lookupFile.Id);
            await fileRepository.SaveAsync();

            if (!fileExists)
            {
                if (issues.Length > 0)
                {
                    issues.Append(", ");
                }

                issues.Append("file does not exist");
                if (!string.IsNullOrEmpty(lookupFile?.Name))
                {
                    issues.Append(": ").Append(lookupFile.Name);
                }
            }

            if (issues.Length > 0)
            {
                throw new OcudaException($"Error: {issues}");
            }
        }

        public async Task DeleteThumbnailAsync(int thumbnailId)
        {
            var thumbnail = await fileThumbnailRepository.FindAsync(thumbnailId);
            fileThumbnailRepository.Remove(thumbnail);
            await fileThumbnailRepository.SaveAsync();
        }

        public async Task EditFileLibraryFileAsync(
            string sectionSlug,
            string fileLibrarySlug,
            File file)
        {
            var currentFile = await fileRepository.FindAsync(file.Id);
            file.FileType = currentFile.FileType;
            var newName = file.Name?.Trim();

            if (currentFile.FileDate != null
                && (file.FileDate == null || file.FileDate == DateTime.MinValue))
            {
                throw new OcudaException("File Date is required for this file.");
            }

            if (currentFile.Name != newName)
            {
                var currentPath = GetFileLibraryFilePath(sectionSlug, fileLibrarySlug, currentFile);
                var newPath = GetFileLibraryFilePath(sectionSlug, fileLibrarySlug, file);

                if (System.IO.File.Exists(newPath))
                {
                    throw new OcudaException("A file already exists in this library with that name.");
                }

                try
                {
                    System.IO.File.Move(currentPath, newPath);
                }
                catch (Exception ex) when (
                    ex is SystemException
                    || ex is System.IO.FileNotFoundException)
                {
                    _logger.LogError(
                        "Unable to move file {SourcePath} to {DestinationPath}: {ErrorMessage}",
                        currentPath,
                        newPath,
                        ex.Message);
                    throw new OcudaException($"Unable to move file on disk: {ex.Message}", ex);
                }
            }

            currentFile.Name = newName;
            currentFile.Description = file.Description?.Trim();
            currentFile.FileDate = file.FileDate;
            currentFile.UpdatedBy = GetCurrentUserId();
            currentFile.UpdatedAt = dateTimeProvider.Now;

            fileRepository.Update(currentFile);
            await fileRepository.SaveAsync();
        }

        public async Task<FileLibrary> EditLibraryTypesAsync(FileLibrary library,
            ICollection<int> fileTypeIds)
        {
            ArgumentNullException.ThrowIfNull(library);

            var currentTypes = await fileTypeService.GetTypesByLibraryIdsAsync(library.Id);
            var currentTypeIds = currentTypes.Select(_ => _.Id).ToList();

            fileTypeIds ??= [];

            var typesToDelete = currentTypeIds.Except(fileTypeIds).ToList();
            var typesToAdd = fileTypeIds.Except(currentTypeIds).ToList();

            await fileLibraryRepository.AddLibraryFileTypesAsync(typesToAdd, library.Id);
            await fileLibraryRepository.RemoveLibraryFileTypesAsync(typesToDelete, library.Id);

            return library;
        }

        public async Task<ICollection<FileType>> GetAllFileTypesAsync()
        {
            return await fileTypeService.GetAllAsync();
        }

        public async Task<File> GetByIdAsync(int id)
        {
            return await fileRepository.FindAsync(id);
        }

        public async Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId)
        {
            return await GetBySectionIdAsync(sectionId, null);
        }

        public async Task<ICollection<FileLibrary>> GetBySectionIdAsync(
            int sectionId,
            bool? isFeatured)
        {
            return await fileLibraryRepository.GetBySectionAsync(sectionId, isFeatured);
        }

        public async Task<FileLibrary> GetBySectionIdSlugAsync(int sectionId, string slug)
        {
            return await fileLibraryRepository.GetBySectionIdSlugAsync(sectionId, slug);
        }

        public async Task<int> GetFileCountAsync(int fileLibraryId)
        {
            var result = await fileRepository.GetPaginatedListAsync(new FilesFilter
            {
                FileLibrary = new FileLibrary
                {
                    Id = fileLibraryId,
                },
                OnlyCount = true,
            });

            return result.Count;
        }

        public async Task<ICollection<FileType>> GetFileLibrariesFileTypesAsync(int libraryId)
        {
            return await fileTypeService.GetTypesByLibraryIdsAsync(libraryId);
        }

        public async Task<File> GetFileLibraryFileAsync(int fileId)
        {
            return await fileRepository.FindAsync(fileId);
        }

        public string GetFileLibraryFilePath(
            string sectionSlug,
            string fileLibrarySlug,
            File file)
        {
            return pathResolver.GetPrivateContentFilePath(
                file.Name + file.FileType.Extension,
                SectionsPath,
                sectionSlug,
                fileLibrarySlug);
        }

        public async Task<FileType> GetFileTypeByIdAsync(int id)
        {
            var types = await GetAllFileTypesAsync();
            return types.FirstOrDefault(_ => _.Id == id);
        }

        public async Task<FileLibrary> GetLibraryByIdAsync(int id)
        {
            return await fileLibraryRepository.FindAsync(id);
        }

        public async Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId)
        {
            return await fileLibraryRepository.GetLibraryFileTypeIdsAsync(libraryId);
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            Section section,
            FilesFilter filter)
        {
            ArgumentNullException.ThrowIfNull(section);

            if (filter?.FileLibrary == null)
            {
                return new DataWithCount<ICollection<File>>
                {
                    Data = [],
                };
            }

            var files = await fileRepository.GetPaginatedListAsync(filter);

            if (filter.OnlyCount)
            {
                return files;
            }

            var userIds = files.Data.Select(_ => _.CreatedBy)
                .Union(files.Data.Where(_ => _.UpdatedBy.HasValue).Select(_ => _.UpdatedBy.Value))
                .Distinct();

            var userLookup = new Dictionary<int, User>();

            foreach (var userId in userIds)
            {
                if (!userLookup.ContainsKey(userId))
                {
                    userLookup.Add(userId, await userService.GetNameUsernameAsync(userId));
                }
            }

            foreach (var file in files.Data)
            {
                file.FileType = await fileTypeService.GetByIdAsync(file.FileTypeId);

                file.CreatedByUser = userLookup.TryGetValue(file.CreatedBy, out User createdUser)
                    ? createdUser
                    : null;

                file.UpdatedByUser = file.UpdatedBy.HasValue
                    && userLookup.TryGetValue(file.UpdatedBy.Value, out User updatedUser)
                        ? updatedUser
                        : null;

                file.Thumbnails
                    .AddRange(await fileThumbnailRepository.GetByFileIdsAsync([file.Id]));

                var filePath = pathResolver
                    .GetPrivateContentFilePath(file.Name + file.FileType.Extension,
                        SectionsPath,
                        section.Slug,
                        filter.FileLibrary.Slug);

                if (System.IO.File.Exists(filePath))
                {
                    file.Size = new System.IO.FileInfo(filePath).Length.ToHumanSizeString();
                }
            }

            return files;
        }

        public string GetPrivateFilePath(File file)
        {
            ArgumentNullException.ThrowIfNull(file);

            return pathResolver
                .GetPrivateContentFilePath($"file{file.Id}{file.FileType.Extension}");
        }

        public string GetPublicFilePath(File file)
        {
            ArgumentNullException.ThrowIfNull(file);

            return pathResolver.GetPublicContentFilePath($"file{file.Id}{file.FileType.Extension}");
        }

        public async Task<string> GetThumbnailPathAsync(
            int thumbnailId,
            string fileLibrarySlug,
            string sectionSlug)
        {
            var thumbnail = await fileThumbnailRepository.FindAsync(thumbnailId)
                ?? throw new OcudaException("Thumbnail not found in database.");

            return GetThumbnailPath(thumbnail.ThumbnailFile, fileLibrarySlug, sectionSlug);
        }

        public async Task<IEnumerable<FileThumbnail>> GetThumbnailsAsync(IEnumerable<int> fileIds)
        {
            return await fileThumbnailRepository.GetByFileIdsAsync(fileIds);
        }

        public string GetUnusedThumbnailPath(
            string filename,
            string fileLibrarySlug,
            string sectionSlug)
        {
            int serialNumber = 1;
            var thumbFilename = filename;
            var path = GetThumbnailPath(filename, fileLibrarySlug, sectionSlug);

            while (System.IO.File.Exists(path))
            {
                filename = System.IO.Path.GetFileNameWithoutExtension(thumbFilename)
                    + $"-{serialNumber++:D2}"
                    + System.IO.Path.GetExtension(filename);

                path = GetThumbnailPath(filename, fileLibrarySlug, sectionSlug);

                if (serialNumber > 99)
                {
                    throw new OcudaException($"Unable to find unused filename for thumbnail, gave up at {filename}");
                }
            }

            return path;
        }

        public async Task<bool> HasReplaceRightsAsync(int fileLibraryId)
        {
            if (IsSiteManager())
            {
                return true;
            }

            var replacePermissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupReplaceFiles>(fileLibraryId);

            return replacePermissions.Any(_ => _.FileLibraryId == fileLibraryId
                && GetPermissionIds().Contains(_.PermissionGroupId));
        }

        public async Task<byte[]> ReadPrivateFileAsync(File file)
        {
            string filePath = GetPrivateFilePath(file);

            await using var fileStream = System.IO.File.OpenRead(filePath);
            await using var ms = new System.IO.MemoryStream();
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<File> ReplaceFileLibraryFileAsync(int fileId)
        {
            var file = await fileRepository.FindAsync(fileId);
            if (file == null)
            {
                _logger.LogError("No file id: {FileId}", fileId);
                throw new OcudaException($"Could not find id: {fileId}");
            }

            file.UpdatedAt = dateTimeProvider.Now;
            file.UpdatedBy = GetCurrentUserId();

            fileRepository.Update(file);
            await fileRepository.SaveAsync();

            return file;
        }

        public async Task UpdateLibrary(Section section, string slug, FileLibrary library)
        {
            ArgumentNullException.ThrowIfNull(section);
            ArgumentNullException.ThrowIfNull(library);

            var currentLibrary = await GetBySectionIdSlugAsync(section.Id, slug);

            if (currentLibrary.Slug != library.Slug.Trim())
            {
                var checkNewSlug = await GetBySectionIdSlugAsync(section.Id, library.Slug.Trim());

                if (checkNewSlug != null)
                {
                    throw new OcudaException(
                        $"The slug {library.Slug.Trim()} is already in use by file library {checkNewSlug.Name}");
                }

                // must move files
                var oldPath = pathResolver.GetPrivateContentFilePath(
                    null,
                    SectionsPath,
                    section.Slug,
                    currentLibrary.Slug);

                var newPath = pathResolver.GetPrivateContentFilePath(
                    null,
                    SectionsPath,
                    section.Slug,
                    library.Slug);

                if (System.IO.Directory.GetFiles(newPath).Length > 0)
                {
                    throw new OcudaException(
                        $"There is already a directory with files in it named with the provided slug {library.Slug.Trim()}.");
                }

                try
                {
                    foreach (var dir in new System.IO.DirectoryInfo(oldPath).GetDirectories())
                    {
                        dir.MoveTo(System.IO.Path.Combine(newPath, dir.Name));
                    }

                    foreach (var file in new System.IO.DirectoryInfo(oldPath).GetFiles())
                    {
                        file.MoveTo(System.IO.Path.Combine(newPath, file.Name));
                    }
                }
                catch (Exception ex)
                {
                    throw new OcudaException($"Unable to move files: {ex.Message}", ex);
                }

                if (System.IO.Directory.GetFiles(oldPath).Length == 0)
                {
                    System.IO.Directory.Delete(oldPath, true);
                }

                currentLibrary.Slug = library.Slug.Trim();
            }

            currentLibrary.IsFeatured = library.IsFeatured;
            currentLibrary.Name = library.Name.Trim();
            currentLibrary.SortOrder = library.SortOrder;
            currentLibrary.UpdatedAt = dateTimeProvider.Now;
            currentLibrary.UpdatedBy = GetCurrentUserId();

            fileLibraryRepository.Update(currentLibrary);
            await fileLibraryRepository.SaveAsync();
        }

        public async Task<string> VerifyAddFileAsync(
            Section section,
            int fileLibraryId,
            string extension,
            string filename)
        {
            var libraryTypes = await GetFileLibrariesFileTypesAsync(fileLibraryId)
                ?? throw new OcudaException(
                    "This file  library is not configured to accept any file types.");

            if (!libraryTypes
                .Any(_ => _.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                throw new OcudaException(
                    $"This file library is not configured to accept files of type: {extension}");
            }

            var library = await fileLibraryRepository.FindAsync(fileLibraryId);

            var filePath = pathResolver.GetPrivateContentFilePath(filename,
                    SectionsPath,
                    section.Slug,
                    library.Slug);

            return System.IO.File.Exists(filePath)
                ? throw new OcudaException(
                    "A file with this name already exists in this file library.")
                : filePath;
        }

        private async Task<File> AddFileLibraryFileInternalAsync(File file, IFormFile fileData)
        {
            var fileLibrary = await fileLibraryRepository.FindAsync(file.FileLibraryId);
            if (fileLibrary == null)
            {
                _logger.LogError("No file library with id: {FileLibraryId}", file.FileLibraryId);
                throw new OcudaException($"Could not find file library id: {file.FileLibraryId}");
            }

            if (fileLibrary.SortOrder == Ops.Models.FileLibrarySort.DocumentDateMonthDescending
                && (file.FileDate == null || file.FileDate == DateTime.MinValue))
            {
                throw new OcudaException("File Date is required for this file.");
            }

            var extension = System.IO.Path.GetExtension(fileData.FileName);
            var fileType = await fileTypeService.GetByExtensionAsync(extension);

            if (fileType == null)
            {
                _logger.LogError("Unknown file type: {Extension}", extension);
                throw new OcudaException($"Unknown file type: {extension}");
            }

            file.CreatedAt = dateTimeProvider.Now;
            file.CreatedBy = GetCurrentUserId();
            file.Description = file.Description?.Trim();
            file.FileTypeId = fileType.Id;
            file.Name = file.Name?.Trim();

            await fileRepository.AddAsync(file);
            await fileRepository.SaveAsync();

            return file;
        }

        private string GetThumbnailPath(
            string thumbnailFile,
            string fileLibrarySlug,
            string sectionSlug)
        {
            return pathResolver.GetPrivateContentFilePath(
                thumbnailFile,
                SectionsPath,
                sectionSlug,
                fileLibrarySlug,
                ThumbnailsPath);
        }
    }
}
