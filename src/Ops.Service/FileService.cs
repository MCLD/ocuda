using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Exceptions;

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
        private readonly IUserRepository _userRepository;
        private readonly IPathResolverService _pathResolver;
        
        public FileService(ILogger<FileService> logger,
            ICategoryRepository categoryRepository,
            IFileRepository fileRepository,
            IPageRepository pageRepository,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository,
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
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
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

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _fileRepository.GetPaginatedListAsync(filter);
        }

        public async Task<File> CreatePrivateFileAsync(int currentUserId, File file, byte[] fileData)
        {
            file.Name = file.Name.Trim();
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = currentUserId;

            await ValidateFileAsync(file);

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            await WritePrivateFileAsync(file, fileData, false);

            _fileRepository.Update(file);
            await _fileRepository.SaveAsync();

            return file;
        }
        public string GetSharedFilePath(File file)
        {
            return _pathResolver.GetPublicContentFilePath($"file{file.Id}{file.Extension}",
                file.Type,
                $"section{file.SectionId}");
        }

        public string GetPrivateFilePath(File file)
        {
            return _pathResolver.GetPrivateContentFilePath($"file{file.Id}{file.Extension}",
                file.Type,
                $"section{file.SectionId}");
        }

        private async Task WritePrivateFileAsync(File file, byte[] fileData, bool isEdit)
        {
            string filePath = GetPrivateFilePath(file);

            if (isEdit)
            {
                _logger.LogInformation($"Editing File (Create): {filePath}");
            }
            else
            {
                _logger.LogInformation($"Writing file: {filePath}");
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileData);
        }

        public async Task<File> EditPrivateFileAsync(File file, byte[] fileData = null)
        {
            var currentFile = await _fileRepository.FindAsync(file.Id);
            currentFile.Name = file.Name.Trim();
            currentFile.Description = file.Description;
            currentFile.CategoryId = file.CategoryId;
            currentFile.IsFeatured = file.IsFeatured;        

            if (fileData != null)
            {
                string oldFilePath = GetPrivateFilePath(currentFile);

                currentFile.Extension = file.Extension;
                currentFile.Icon = file.Icon;

                await ValidateFileAsync(currentFile);

                if (System.IO.File.Exists(oldFilePath))
                {
                    _logger.LogInformation($"Editing File (Delete): {oldFilePath}");
                    System.IO.File.Delete(oldFilePath);
                }              

                await WritePrivateFileAsync(currentFile, fileData, true);
            }
            else
            {
                await ValidateFileAsync(currentFile);
            }

            _fileRepository.Update(currentFile);
            await _fileRepository.SaveAsync();
            return currentFile;
        }

        public async Task DeletePrivateFileAsync(int id)
        {
            var file = await _fileRepository.FindAsync(id);

            string filePath = GetPrivateFilePath(file);

            if (System.IO.File.Exists(filePath))
            {
                _logger.LogInformation($"Deleting file: {filePath}");
                System.IO.File.Delete(filePath);
            }

            _fileRepository.Remove(id);
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

            if (string.IsNullOrWhiteSpace(file.Type))
            {
                message = $"File must have a type.";
                _logger.LogWarning(message, file);
                throw new OcudaException(message);
            }

            if (file.CategoryId.HasValue)
            {
                var category = await _categoryRepository.FindAsync(file.CategoryId.Value);
                if (category == null)
                {
                    message = $"CategoryId '{file.CategoryId}' is not valid.";
                    _logger.LogWarning(message, file.CategoryId);
                    throw new OcudaException(message);
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
    }
}
