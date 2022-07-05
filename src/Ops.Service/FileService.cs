using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class FileService : BaseService<FileService>, IFileService
    {
        private const string SectionsPath = "sections";

        private readonly IFileLibraryRepository _fileLibraryRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileTypeService _fileTypeService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPathResolverService _pathResolver;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISectionService _sectionService;

        public FileService(ILogger<FileService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFileLibraryRepository fileLibraryRepository,
            IFileRepository fileRepository,
            IFileTypeService fileTypeService,
            IPathResolverService pathResolver,
            IPermissionGroupService permissionGroupService,
            ISectionService sectionService,
            IWebHostEnvironment hostingEnvironment) : base(logger, httpContextAccessor)
        {
            _fileLibraryRepository = fileLibraryRepository
                ?? throw new ArgumentNullException(nameof(fileLibraryRepository));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _fileTypeService = fileTypeService
                ?? throw new ArgumentNullException(nameof(fileTypeService));
            _hostingEnvironment = hostingEnvironment
                ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public Task<File> AddFileLibraryFileAsync(File file, IFormFile fileData)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (fileData == null)
            {
                throw new ArgumentNullException(nameof(fileData));
            }

            return AddFileLibraryFileInternalAsync(file, fileData);
        }

        public async Task<FileLibrary> CreateLibraryAsync(FileLibrary library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            library.Name = library.Name?.Trim();
            library.Slug = library.Slug?.Trim();

            var exists = await _fileLibraryRepository
                .GetBySectionIdSlugAsync(library.SectionId, library.Slug);

            if (exists != null)
            {
                throw new OcudaException($"A file library for this section already exists with this stub: {library.Slug}");
            }

            var section = await _sectionService.GetByIdAsync(library.SectionId);

            var path = _pathResolver
                .GetPrivateContentFilePath(null, SectionsPath, section.Slug, library.Slug);

            var directory = new System.IO.DirectoryInfo(path);

            if (directory.GetFiles().Length > 0)
            {
                throw new OcudaException($"Directory already exists and has {directory.GetFiles().Length} files in it.");
            }

            library.CreatedAt = DateTime.Now;
            library.CreatedBy = GetCurrentUserId();
            await _fileLibraryRepository.AddAsync(library);
            await _fileLibraryRepository.SaveAsync();

            return library;
        }

        public async Task<File> CreatePublicFileAsync(File file, IFormFile fileData)
        {
            var extension = System.IO.Path.GetExtension(fileData.FileName);
            var fileType = await _fileTypeService.GetByExtensionAsync(extension);

            if (fileType == null)
            {
                _logger.LogError("{Extension} is an unknown file type.", extension);
                throw new OcudaException("Unknown file type.");
            }

            file.Name = file.Name?.Trim();
            file.Description = file.Description?.Trim();
            file.FileTypeId = fileType.Id;
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = GetCurrentUserId();

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            file.FileType = fileType;

            await WritePublicFileAsync(file, fileData);

            return file;
        }

        public async Task DeleteFileTypesByLibrary(int fileLibraryId)
        {
            var currentLibrary = await _fileLibraryRepository.FindAsync(fileLibraryId);
            var fileTypeIds = await GetLibraryFileTypeIdsAsync(fileLibraryId);
            var fileTypesToRemove = currentLibrary.FileTypes
                .Where(_ => fileTypeIds.Any(__ => __ == _.FileTypeId))
                .Select(_ => _.FileTypeId)
                .ToList();
            await _fileLibraryRepository
                .RemoveLibraryFileTypesAsync(fileTypesToRemove, currentLibrary.Id);
        }

        public async Task DeleteLibraryAsync(int sectionId, int fileLibraryId)
        {
            var library = await GetLibraryByIdAsync(fileLibraryId);
            var section = await _sectionService.GetByIdAsync(sectionId);

            var filePath = _pathResolver
                .GetPrivateContentFilePath(null, SectionsPath, section.Slug, library.Slug);

            var exists = System.IO.Directory.Exists(filePath);

            if (exists)
            {
                System.IO.Directory.Delete(filePath);
            }
            await DeleteFileTypesByLibrary(fileLibraryId);
            _fileLibraryRepository.Remove(fileLibraryId);
            await _fileLibraryRepository.SaveAsync();

            if (!exists)
            {
                throw new OcudaException($"Directory does not exist: {System.IO.Path.GetFileName(filePath)}");
            }
        }

        public async Task DeletePrivateFileAsync(int sectionId,
            string fileLibraryStub,
            int fileId)
        {
            var filePath = await GetFilePathAsync(sectionId, fileLibraryStub, fileId);

            var fileExists = System.IO.File.Exists(filePath);

            if (fileExists)
            {
                System.IO.File.Delete(filePath);
            }

            _fileRepository.Remove(fileId);
            await _fileRepository.SaveAsync();

            if (!fileExists)
            {
                var file = await _fileRepository.FindAsync(fileId);
                throw new OcudaException($"File does not exist: {file.Name}");
            }
        }

        public async Task DeletePublicFileAsync(int id)
        {
            var file = await _fileRepository.FindAsync(id);

            string filePath = GetPublicFilePath(file);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _fileRepository.Remove(id);
            await _fileRepository.SaveAsync();
        }

        public async Task<FileLibrary> EditLibraryTypesAsync(FileLibrary library,
            ICollection<int> fileTypeIds)
        {
            var currentTypes = await _fileTypeService.GetTypesByLibraryIdsAsync(library.Id);
            var currentTypeIds = currentTypes.Select(_ => _.Id).ToList();

            if (fileTypeIds == null)
            {
                fileTypeIds = new List<int>();
            }

            var typesToDelete = currentTypeIds.Except(fileTypeIds).ToList();
            var typessToAdd = fileTypeIds.Except(currentTypeIds).ToList();

            await _fileLibraryRepository.AddLibraryFileTypesAsync(typessToAdd, library.Id);
            await _fileLibraryRepository.RemoveLibraryFileTypesAsync(typesToDelete, library.Id);

            return library;
        }

        public async Task<File> EditPrivateFileAsync(File file,
            IFormFile fileData,
            ICollection<IFormFile> thumbnailFiles,
            int[] thumbnailIdsToKeep)
        {
            var currentFile = await _fileRepository.FindAsync(file.Id);

            if (fileData != null)
            {
                var extension = System.IO.Path.GetExtension(fileData.FileName);
                var fileType = await _fileTypeService.GetByExtensionAsync(extension);

                if (fileType == null)
                {
                    _logger.LogError("{Extension} is an unknown file type.", extension);
                    throw new OcudaException("Unknown file type.");
                }

                currentFile.FileTypeId = fileType.Id;
                currentFile.FileType = fileType;
            }

            currentFile.Description = file.Description?.Trim();
            currentFile.Name = file.Name?.Trim();
            currentFile.UpdatedAt = DateTime.Now;
            currentFile.UpdatedBy = GetCurrentUserId();

            _fileRepository.Update(currentFile);
            await _fileRepository.SaveAsync();

            if (fileData != null)
            {
                var currentFilePath = GetPrivateFilePath(currentFile);
                await WritePrivateFileAsync(currentFile, fileData, currentFilePath);
            }

            return currentFile;
        }

        public async Task<ICollection<int>> GetAllFileTypeIdsAsync()
        {
            return await _fileTypeService.GetAllIdsAsync();
        }

        public async Task<ICollection<FileType>> GetAllFileTypesAsync()
        {
            return await _fileTypeService.GetAllAsync();
        }

        public async Task<File> GetByIdAsync(int id)
        {
            return await _fileRepository.FindAsync(id);
        }

        public async Task<ICollection<FileLibrary>> GetBySectionIdAsync(int sectionId)
        {
            return await _fileLibraryRepository.GetBySectionIdAsync(sectionId);
        }

        public async Task<FileLibrary> GetBySectionIdSlugAsync(int sectionId, string stub)
        {
            return await _fileLibraryRepository.GetBySectionIdSlugAsync(sectionId, stub);
        }

        public async Task<ICollection<FileType>> GetFileLibrariesFileTypesAsync(int libraryId)
        {
            return await _fileTypeService.GetTypesByLibraryIdsAsync(libraryId);
        }

        public async Task<string> GetFilePathAsync(int sectionId, string libraryStub, int fileId)
        {
            var section = await _sectionService.GetByIdAsync(sectionId);
            var file = await GetByIdAsync(fileId);

            return _pathResolver
                .GetPrivateContentFilePath(file.Name + file.FileType.Extension,
                    SectionsPath,
                    section.Slug,
                    libraryStub);
        }

        public async Task<FileType> GetFileTypeByIdAsync(int id)
        {
            var types = await GetAllFileTypesAsync();
            return types.FirstOrDefault(_ => _.Id == id);
        }

        public async Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId)
        {
            return await _fileRepository.GetFileTypeIdsInUseByLibraryAsync(libraryId);
        }

        public async Task<FileLibrary> GetLibraryByIdAsync(int id)
        {
            return await _fileLibraryRepository.FindAsync(id);
        }

        public async Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId)
        {
            return await _fileLibraryRepository.GetLibraryFileTypeIdsAsync(libraryId);
        }

        public async Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter)
        {
            return await _fileLibraryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter)
        {
            if (!filter.FileLibraryId.HasValue)
            {
                return new DataWithCount<ICollection<File>>
                {
                    Data = new List<File>(),
                    Count = 0
                };
            }

            var library = await GetLibraryByIdAsync(filter.FileLibraryId.Value);
            var section = await _sectionService.GetByIdAsync(library.SectionId);
            var files = await _fileRepository.GetPaginatedListAsync(filter);

            foreach (var file in files.Data)
            {
                var filePath = _pathResolver
                    .GetPrivateContentFilePath(file.Name + file.FileType.Extension,
                        SectionsPath,
                        section.Slug,
                        library.Slug);

                if (System.IO.File.Exists(filePath))
                {
                    file.Size = new System.IO.FileInfo(filePath).HumanSize();
                }
            }
            return files;
        }

        public string GetPrivateFilePath(File file)
        {
            return _pathResolver.GetPrivateContentFilePath($"file{file.Id}{file.FileType.Extension}");
        }

        public string GetPublicFilePath(File file)
        {
            return _pathResolver.GetPublicContentFilePath($"file{file.Id}{file.FileType.Extension}");
        }

        public async Task<bool> HasReplaceRightsAsync(int fileLibraryId)
        {
            var replacePermissions = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupReplaceFiles>(fileLibraryId);

            return replacePermissions.Any(_ => _.FileLibraryId == fileLibraryId
                && GetPermissionIds().Contains(_.PermissionGroupId));
        }

        public async Task<byte[]> ReadPrivateFileAsync(File file)
        {
            string filePath = GetPrivateFilePath(file);

            using var fileStream = System.IO.File.OpenRead(filePath);
            using var ms = new System.IO.MemoryStream();
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<File> ReplaceFileLibraryFileAsync(int fileId)
        {
            var file = await _fileRepository.FindAsync(fileId);
            if (file == null)
            {
                _logger.LogError("No file id: {FileId}", fileId);
                throw new OcudaException($"Could not find id: {fileId}");
            }

            file.UpdatedAt = DateTime.Now;
            file.UpdatedBy = GetCurrentUserId();

            _fileRepository.Update(file);
            await _fileRepository.SaveAsync();

            return file;
        }

        public async Task UpdateLibrary(FileLibrary library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            var currentLibrary = await GetLibraryByIdAsync(library.Id);

            if (currentLibrary.Slug.Trim() != library.Slug.Trim())
            {
                // must move files
                var oldPath = _pathResolver.GetPrivateContentFilePath(null,
                    SectionsPath,
                    currentLibrary.Section.Slug,
                    currentLibrary.Slug);

                var newPath = _pathResolver.GetPrivateContentFilePath(null,
                    SectionsPath,
                    currentLibrary.Section.Slug,
                    library.Slug);

                if (System.IO.Directory.GetFiles(newPath).Length > 0)
                {
                    throw new OcudaException("There is already a directory with files in it named with the provided stub.");
                }

                try
                {
                    System.IO.Directory.Move(oldPath, newPath);
                }
                catch (Exception ex)
                {
                    throw new OcudaException($"Unable to move files: {ex.Message}", ex);
                }
            }

            currentLibrary.Name = library.Name.Trim();
            currentLibrary.Slug = library.Slug.Trim();
            currentLibrary.UpdatedAt = DateTime.Now;
            currentLibrary.UpdatedBy = GetCurrentUserId();

            _fileLibraryRepository.Update(currentLibrary);

            await _fileLibraryRepository.SaveAsync();
        }

        public async Task<string> VerifyAddFileAsync(int fileLibraryId,
            string extension,
            string filename)
        {
            var libraryTypes = await GetFileLibrariesFileTypesAsync(fileLibraryId);
            if (libraryTypes == null)
            {
                throw new OcudaException("This file library is not configured to accept any file types.");
            }

            if (!libraryTypes.Any(_ => _.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                throw new OcudaException($"This file library is not configured to accept files of type: {extension}");
            }

            var library = await _fileLibraryRepository.FindAsync(fileLibraryId);
            var section = await _sectionService.GetByIdAsync(library.SectionId);

            var filePath = _pathResolver.GetPrivateContentFilePath(filename,
                    SectionsPath,
                    section.Slug,
                    library.Slug);

            if (System.IO.File.Exists(filePath))
            {
                throw new OcudaException("A file with this name already exists in this file library.");
            }

            return filePath;
        }

        private async Task<File> AddFileLibraryFileInternalAsync(File file, IFormFile fileData)
        {
            var fileLibrary = await _fileLibraryRepository.FindAsync(file.FileLibraryId);
            if (fileLibrary == null)
            {
                _logger.LogError("No file library with id: {FileLibraryId}", file.FileLibraryId);
                throw new OcudaException($"Could not find file library id: {file.FileLibraryId}");
            }

            var extension = System.IO.Path.GetExtension(fileData.FileName);
            var fileType = await _fileTypeService.GetByExtensionAsync(extension);

            if (fileType == null)
            {
                _logger.LogError("Unknown file type: {Extension}", extension);
                throw new OcudaException($"Unknown file type: {extension}");
            }

            file.CreatedAt = DateTime.Now;
            file.CreatedBy = GetCurrentUserId();
            file.Description = file.Description?.Trim();
            file.FileTypeId = fileType.Id;
            file.Name = file.Name?.Trim();

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            return file;
        }

        private async Task WritePrivateFileAsync(File file, IFormFile fileData,
                            string oldFilePath = null)
        {
            string filePath = GetPrivateFilePath(file);
            byte[] fileBytes = await FormFileHelper.GetFileBytesAsync(fileData);

            if (!string.IsNullOrWhiteSpace(oldFilePath) && System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }

        private async Task WritePublicFileAsync(File file, IFormFile fileData,
            string oldFilePath = null)
        {
            string filePath = GetPublicFilePath(file);
            byte[] fileBytes = await FormFileHelper.GetFileBytesAsync(fileData);

            if (!string.IsNullOrWhiteSpace(oldFilePath) && System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }
    }
}
