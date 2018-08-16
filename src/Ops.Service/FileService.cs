using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
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
        private readonly ICategoryRepository _categoryRepository;
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
            ICategoryRepository categoryRepository,
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
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
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
            file.Name = file.Name?.Trim();
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = currentUserId;
            file.Extension = System.IO.Path.GetExtension(fileData.FileName);
            file.FileTypeId = await _fileTypeService.GetIdByExtensionAsync(file.Extension);

            if (thumbnailFiles != null)
            {
                var thumbnailList = new List<Thumbnail>();

                foreach (var thumbnail in thumbnailFiles)
                {
                    thumbnailList.Add(new Thumbnail
                    {
                        Name = thumbnail.FileName,
                        CreatedAt = file.CreatedAt,
                        CreatedBy = file.CreatedBy
                    });
                }

                file.Thumbnails = thumbnailList;
            }

            await ValidateFileAsync(file);

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            await WritePrivateFileAsync(file, fileData);

            if (thumbnailFiles != null)
            {
                await _thumbnailService.CreateThumbnailFilesAsync(file.Thumbnails, thumbnailFiles);
            }

            return file;
        }

        public async Task<File> EditPrivateFileAsync(int currentUserId,
            File file, IFormFile fileData, ICollection<IFormFile> thumbnailFiles, int[] thumbnailIdsToKeep)
        {
            var currentFile = await _fileRepository.FindAsync(file.Id);
            var currentFilePath = GetPrivateFilePath(currentFile);

            currentFile.Name = file.Name?.Trim();
            currentFile.Description = file.Description;
            currentFile.IsFeatured = file.IsFeatured;

            if (fileData != null)
            {
                currentFile.Extension = System.IO.Path.GetExtension(fileData.FileName);
                currentFile.FileTypeId = await _fileTypeService.GetIdByExtensionAsync(file.Extension);
            }

            var thumbnailsToRemove = currentFile.Thumbnails
                .Where(_ => thumbnailIdsToKeep.Contains(_.Id) == false).ToList();

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
                        Name = thumbnail.FileName,
                        CreatedAt = DateTime.Now,
                        CreatedBy = currentUserId
                    };

                    thumbnailsToAdd.Add(newThumbnail);
                    currentFile.Thumbnails.Add(newThumbnail);
                }
            }

            await ValidateFileAsync(currentFile);

            _fileRepository.Update(currentFile);
            await _fileRepository.SaveAsync();
            await _thumbnailRepository.SaveAsync();

            if (fileData != null)
            {
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
            return _pathResolver.GetPublicContentFilePath($"file{file.Id}{file.Extension}",
                $"section{file.SectionId}");
        }

        public string GetPrivateFilePath(File file)
        {
            return _pathResolver.GetPrivateContentFilePath($"file{file.Id}{file.Extension}",
                $"section{file.SectionId}");
        }

        private async Task WritePrivateFileAsync(File file, IFormFile fileData, string oldFilePath = null)
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

        public async Task ValidateFileAsync(File file)
        {
            // TODO Update Validation
            var message = string.Empty;
            var section = await _sectionRepository.FindAsync(file.SectionId);

            if (section == null)
            {
                message = $"SectionId '{file.SectionId}' is not a valid section.";
                _logger.LogWarning(message, file.SectionId);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(file.Name))
            {
                message = $"File name cannot be empty.";
                _logger.LogWarning(message, file);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(file.Extension))
            {
                message = $"File must have a type extension.";
                _logger.LogWarning(message, file);
                throw new OcudaException(message);
            }

            //if (file.FileTypeId)
            //{
            //    message = $"File must have a type.";
            //    _logger.LogWarning(message, file);
            //    throw new OcudaException(message);
            //}

            if (file.CategoryId.HasValue)
            {
                var category = await _categoryRepository
                    .GetCategoryAndFileTypesByCategoryIdAsync(file.CategoryId.Value);

                if (category == null)
                {
                    message = $"CategoryId '{file.CategoryId}' is not valid.";
                    _logger.LogWarning(message, file.CategoryId);
                    throw new OcudaException(message);
                }
                else
                {
                    var fileExtensions = category.CategoryFileTypes.Select(_ => _.FileType.Extension);

                    if (fileExtensions.Count() > 0 && !fileExtensions.Contains(file.Extension))
                    {
                        message = $"{category.Name} does not accept files ending with '{file.Extension}'.";
                        _logger.LogWarning(message, file.CategoryId, file.Extension, fileExtensions);
                        throw new OcudaException(message);
                    }
                }
            }

            if (file.PageId.HasValue)
            {
                var page = await _pageRepository.FindAsync(file.PageId.Value);
                if (page == null)
                {
                    message = $"PageId '{file.PageId}' is not valid.";
                    _logger.LogWarning(message, file.PageId);
                    throw new OcudaException(message);
                }
            }

            if (file.PostId.HasValue)
            {
                var post = await _postRepository.FindAsync(file.PostId.Value);
                if (post == null)
                {
                    message = $"PostId '{file.PostId}' is not valid.";
                    _logger.LogWarning(message, file.PostId);
                    throw new OcudaException(message);
                }
            }

            if (!file.CategoryId.HasValue && !file.PageId.HasValue && !file.PostId.HasValue)
            {
                message = $"File must be assigned to a Category, Page, or Post";
                _logger.LogWarning(message, file);
                throw new OcudaException(message);
            }

            var creator = await _userRepository.FindAsync(file.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {file.CreatedBy}";
                _logger.LogWarning(message, file.CreatedBy);
                throw new OcudaException(message);
            }
        }

        public async Task<IEnumerable<int>> GetFileTypeIdsInUseByCategoryIdAsync(int categoryId)
        {
            return await _fileRepository.GetFileTypeIdsInUseByCategoryId(categoryId);
        }

        public async Task<File> CreatePublicFileAsync(int currentUserId, File file, IFormFile fileData)
        {
            file.Name = file.Name?.Trim();
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = currentUserId;
            file.Extension = System.IO.Path.GetExtension(fileData.FileName);
            file.FileTypeId = await _fileTypeService.GetIdByExtensionAsync(file.Extension);

            await ValidateFileAsync(file);

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

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
    }
}
