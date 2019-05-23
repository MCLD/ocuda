using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Service
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IFileLibraryRepository _fileLibraryRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IThumbnailRepository _thumbnailRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileTypeService _fileTypeService;
        private readonly IThumbnailService _thumbnailService;
        private readonly IPathResolverService _pathResolver;

        public FileService(ILogger<FileService> logger,
            IFileLibraryRepository fileLibraryRepository,
            IFileRepository fileRepository,
            IPageRepository pageRepository,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IThumbnailRepository thumbnailRepository,
            IUserRepository userRepository,
            IFileTypeService fileTypeService,
            IThumbnailService thumbnailService,
            IPathResolverService pathResolver)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _fileLibraryRepository = fileLibraryRepository
                ?? throw new ArgumentNullException(nameof(fileLibraryRepository));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _postRepository = postRepository
                ?? throw new ArgumentNullException(nameof(postRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _thumbnailRepository = thumbnailRepository
                ?? throw new ArgumentNullException(nameof(thumbnailRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
            _fileTypeService = fileTypeService
                ?? throw new ArgumentNullException(nameof(fileTypeService));
            _thumbnailService = thumbnailService
               ?? throw new ArgumentNullException(nameof(thumbnailService));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public async Task<int> GetFileCountAsync()
        {
            return await _fileRepository.CountAsync();
        }

        public async Task<ICollection<File>> GetFilesAsync()
        {
            return await _fileRepository.ToListAsync(_ => _.Name);
        }

        public async Task<File> GetByIdAsync(int id)
        {
            return await _fileRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            BlogFilter filter, bool isGallery = false)
        {
            return await _fileRepository.GetPaginatedListAsync(filter, isGallery);
        }

        public async Task<File> CreatePrivateFileAsync(int currentUserId,
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles)
        {
            var extension = System.IO.Path.GetExtension(fileData.FileName);
            var fileType = await _fileTypeService.GetByExtensionAsync(extension);

            if (fileType == null)
            {
                _logger.LogError($"{extension} is an unknown file type.", file);
                throw new OcudaException("Unknown file type.");
            }

            file.Description = file.Description?.Trim();
            file.Name = file.Name?.Trim();
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = currentUserId;
            file.FileTypeId = fileType.Id;

            if (thumbnailFiles != null)
            {
                var thumbnailList = new List<Thumbnail>();

                foreach (var thumbnail in thumbnailFiles)
                {
                    thumbnailList.Add(new Thumbnail
                    {
                        CreatedAt = file.CreatedAt,
                        CreatedBy = file.CreatedBy
                    });
                }

                file.Thumbnails = thumbnailList;
            }

            ValidateFile(file);

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            file.FileType = fileType;
            await WritePrivateFileAsync(file, fileData);

            if (thumbnailFiles != null)
            {
                await _thumbnailService.CreateThumbnailFilesAsync(file.Thumbnails, thumbnailFiles);
            }

            return file;
        }

        public async Task<File> EditPrivateFileAsync(int currentUserId,
            File file,
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
                    _logger.LogError($"{extension} is an unknown file type.", file);
                    throw new OcudaException("Unknown file type.");
                }

                currentFile.FileTypeId = fileType.Id;
                currentFile.FileType = fileType;
            }

            currentFile.Description = file.Description?.Trim();
            currentFile.Name = file.Name?.Trim();

            var thumbnailsToRemove = currentFile.Thumbnails
                .Where(_ => !thumbnailIdsToKeep.Contains(_.Id)).ToList();

            foreach (var thumbnail in thumbnailsToRemove)
            {
                currentFile.Thumbnails.Remove(thumbnail);
                _thumbnailRepository.Remove(thumbnail);
            }

            var thumbnailsToAdd = new List<Thumbnail>();

            if (thumbnailFiles != null)
            {
                foreach (var thumbnail in thumbnailFiles)
                {
                    var newThumbnail = new Thumbnail
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = currentUserId
                    };

                    thumbnailsToAdd.Add(newThumbnail);
                    currentFile.Thumbnails.Add(newThumbnail);
                }
            }

            ValidateFile(currentFile);

            _fileRepository.Update(currentFile);
            await _fileRepository.SaveAsync();
            await _thumbnailRepository.SaveAsync();

            if (fileData != null)
            {
                var currentFilePath = GetPrivateFilePath(currentFile);
                await WritePrivateFileAsync(currentFile, fileData, currentFilePath);
            }

            if (thumbnailsToRemove.Count > 0)
            {
                _thumbnailService.DeleteThumbnailFiles(thumbnailsToRemove);
            }

            if (thumbnailsToAdd.Count > 0)
            {
                await _thumbnailService.CreateThumbnailFilesAsync(thumbnailsToAdd, thumbnailFiles);
            }

            return currentFile;
        }

        public string GetPublicFilePath(File file)
        {
            return _pathResolver.GetPublicContentFilePath($"file{file.Id}{file.FileType.Extension}");
        }

        public string GetPrivateFilePath(File file)
        {
            return _pathResolver.GetPrivateContentFilePath($"file{file.Id}{file.FileType.Extension}");
        }

        private async Task WritePrivateFileAsync(File file, IFormFile fileData,
            string oldFilePath = null)
        {
            string filePath = GetPrivateFilePath(file);
            byte[] fileBytes = IFormFileHelper.GetFileBytes(fileData);

            if (string.IsNullOrWhiteSpace(oldFilePath))
            {
                _logger.LogInformation($"Writing file: {filePath}");
            }
            else
            {
                if (System.IO.File.Exists(oldFilePath))
                {
                    _logger.LogInformation($"Editing File (Delete): {oldFilePath}");
                    System.IO.File.Delete(oldFilePath);
                }

                _logger.LogInformation($"Editing File (Create): {filePath}");
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }

        public async Task DeletePrivateFileAsync(int id)
        {
            var file = await _fileRepository.FindAsync(id);

            if (file.Thumbnails.Count > 0)
            {
                _thumbnailService.DeleteThumbnailFiles(file.Thumbnails);
            }

            string filePath = GetPrivateFilePath(file);

            if (System.IO.File.Exists(filePath))
            {
                _logger.LogInformation($"Deleting file: {filePath}");
                System.IO.File.Delete(filePath);
            }

            _fileRepository.Remove(id);
            _thumbnailRepository.RemoveByFileId(id);
            await _fileRepository.SaveAsync();
        }

        public async Task<byte[]> ReadPrivateFileAsync(File file)
        {
            string filePath = GetPrivateFilePath(file);

            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    await fileStream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }

        public async Task<File> CreatePublicFileAsync(int currentUserId, File file, IFormFile fileData)
        {
            var extension = System.IO.Path.GetExtension(fileData.FileName);
            var fileType = await _fileTypeService.GetByExtensionAsync(extension);

            if (fileType == null)
            {
                _logger.LogError($"{extension} is an unknown file type.", file);
                throw new OcudaException("Unknown file type.");
            }

            file.Name = file.Name?.Trim();
            file.Description = file.Description?.Trim();
            file.FileTypeId = fileType.Id;
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = currentUserId;


            ValidateFile(file);

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            file.FileType = fileType;

            await WritePublicFileAsync(file, fileData);

            return file;
        }

        private async Task WritePublicFileAsync(File file, IFormFile fileData, string oldFilePath = null)
        {
            string filePath = GetPublicFilePath(file);
            byte[] fileBytes = IFormFileHelper.GetFileBytes(fileData);

            if (string.IsNullOrWhiteSpace(oldFilePath))
            {
                _logger.LogInformation($"Writing file: {filePath}");
            }
            else
            {
                if (System.IO.File.Exists(oldFilePath))
                {
                    _logger.LogInformation($"Editing File (Delete): {oldFilePath}");
                    System.IO.File.Delete(oldFilePath);
                }

                _logger.LogInformation($"Editing File (Create): {filePath}");
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }

        public async Task<IEnumerable<File>> GetByPageIdAsync(int pageId)
        {
            return await _fileRepository.GetByPageIdAsync(pageId);
        }

        public async Task<IEnumerable<File>> GetByPostIdAsync(int postId)
        {
            return await _fileRepository.GetByPostIdAsync(postId);
        }

        public async Task DeletePublicFileAsync(int id)
        {
            var file = await _fileRepository.FindAsync(id);

            string filePath = GetPublicFilePath(file);

            if (System.IO.File.Exists(filePath))
            {
                _logger.LogInformation($"Deleting file: {filePath}");
                System.IO.File.Delete(filePath);
            }

            _fileRepository.Remove(id);
            await _fileRepository.SaveAsync();
        }

        private void ValidateFile(File file)
        {
            if (file.FileLibraryId.HasValue)
            {
                if (file.PageId.HasValue)
                {
                    var message = "File cannot belong to a file library and a page.";
                    _logger.LogWarning(message, file);
                    throw new OcudaException(message);
                }
                else if (file.PostId.HasValue)
                {
                    var message = "File cannot belong to a file library and a post.";
                    _logger.LogWarning(message, file);
                    throw new OcudaException(message);
                }
            }
            else if (file.PageId.HasValue && file.PostId.HasValue)
            {
                var message = "File cannot belong to a page and a post.";
                _logger.LogWarning(message, file);
                throw new OcudaException(message);
            }
            else if (!file.PageId.HasValue && !file.PostId.HasValue)
            {
                var message = "File must belong to a file library, page or post.";
                _logger.LogWarning(message, file);
                throw new OcudaException(message);
            }
        }

        public async Task<FileLibrary> GetLibraryByIdAsync(int id)
        {
            return await _fileLibraryRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter)
        {
            return await _fileLibraryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<FileLibrary> CreateLibraryAsync(int currentUserId, FileLibrary library,
            ICollection<int> fileTypeIds)
        {
            library.Name = library.Name?.Trim();
            library.CreatedAt = DateTime.Now;
            library.CreatedBy = currentUserId;
            library.FileTypes = fileTypeIds.Select(_ => new FileLibraryFileType
            {
                FileTypeId = _
            }).ToList();

            await _fileLibraryRepository.AddAsync(library);
            await _fileLibraryRepository.SaveAsync();

            return library;
        }

        public async Task<FileLibrary> EditLibraryAsync(FileLibrary library,
            ICollection<int> fileTypeIds)
        {
            var currentLibrary = await _fileLibraryRepository.FindAsync(library.Id);

            currentLibrary.Name = currentLibrary.Name.Trim();

            var fileTypesToAdd = fileTypeIds
                .Except(currentLibrary.FileTypes.Select(_ => _.FileTypeId))
                .Select(_ => new FileLibraryFileType
                {
                    FileLibrary = currentLibrary,
                    FileTypeId = _
                });
            foreach (var fileType in fileTypesToAdd)
            {
                currentLibrary.FileTypes.Add(fileType);
            }

            var fileTypesToRemove = currentLibrary.FileTypes
                .Where(_ => !fileTypeIds.Contains(_.FileTypeId));

            _fileLibraryRepository.Update(currentLibrary);
            _fileLibraryRepository.RemoveLibraryFileTypes(fileTypesToRemove);
            await _fileLibraryRepository.SaveAsync();

            return library;
        }

        public async Task DeleteLibraryAsync(int id)
        {
            _fileLibraryRepository.Remove(id);
            await _fileLibraryRepository.SaveAsync();
        }

        public async Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId)
        {
            return await _fileLibraryRepository.GetLibraryFileTypeIdsAsync(libraryId);
        }

        public async Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId)
        {
            return await _fileRepository.GetFileTypeIdsInUseByLibraryAsync(libraryId);
        }
    }
}
