﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class FileService : BaseService<FileService>, IFileService
    {
        private readonly IFileLibraryRepository _fileLibraryRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileTypeService _fileTypeService;
        private readonly IPathResolverService _pathResolver;

        public FileService(ILogger<FileService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFileLibraryRepository fileLibraryRepository,
            IFileRepository fileRepository,
            IFileTypeService fileTypeService,
            IPathResolverService pathResolver)
            : base(logger, httpContextAccessor)
        {
            _fileLibraryRepository = fileLibraryRepository
                ?? throw new ArgumentNullException(nameof(fileLibraryRepository));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _fileTypeService = fileTypeService
                ?? throw new ArgumentNullException(nameof(fileTypeService));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public async Task<File> GetByIdAsync(int id)
        {
            return await _fileRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _fileRepository.GetPaginatedListAsync(filter);
        }

        public async Task<File> CreatePrivateFileAsync(File file, IFormFile fileDatas)
        {
            var extension = System.IO.Path.GetExtension(fileDatas.FileName);
            var fileType = await _fileTypeService.GetByExtensionAsync(extension);

            if (fileType == null)
            {
                _logger.LogError("{Extension} is an unknown file type.", extension);
                throw new OcudaException("Unknown file type.");
            }

            file.Description = file.Description?.Trim();
            file.Name = file.Name?.Trim();
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = GetCurrentUserId();
            file.FileTypeId = fileType.Id;

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            file.FileType = fileType;
            await WritePrivateFileAsync(file, fileDatas);

            return file;
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

            if (!string.IsNullOrWhiteSpace(oldFilePath) && System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }

        public async Task DeletePrivateFileAsync(int id)
        {
            var file = await _fileRepository.FindAsync(id);

            string filePath = GetPrivateFilePath(file);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _fileRepository.Remove(id);
            await _fileRepository.SaveAsync();
        }

        public async Task<byte[]> ReadPrivateFileAsync(File file)
        {
            string filePath = GetPrivateFilePath(file);

            using var fileStream = System.IO.File.OpenRead(filePath);
            using var ms = new System.IO.MemoryStream();
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
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

        private async Task WritePublicFileAsync(File file, IFormFile fileData,
            string oldFilePath = null)
        {
            string filePath = GetPublicFilePath(file);
            byte[] fileBytes = IFormFileHelper.GetFileBytes(fileData);

            if (!string.IsNullOrWhiteSpace(oldFilePath) && System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
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

        public async Task<FileLibrary> GetLibraryByIdAsync(int id)
        {
            return await _fileLibraryRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter)
        {
            return await _fileLibraryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<FileLibrary> CreateLibraryAsync(FileLibrary library, int sectionId)
        {
            library.Name = library.Name?.Trim();
            library.Stub = library.Stub?.Trim();
            library.CreatedAt = DateTime.Now;
            library.CreatedBy = GetCurrentUserId();

            await _fileLibraryRepository.AddAsync(library);
            await _fileLibraryRepository.SaveAsync();

            return library;
        }

        public async Task UpdateLibrary(FileLibrary library)
        {
            library.Name = library.Name?.Trim();
            library.Stub = library.Stub?.Trim();
            library.UpdatedAt = DateTime.Now;
            library.UpdatedBy = GetCurrentUserId();

            _fileLibraryRepository.Update(library);

            await _fileLibraryRepository.SaveAsync();
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

        public async Task DeleteLibraryAsync(int id)
        {
            _fileLibraryRepository.Remove(id);
            await _fileLibraryRepository.SaveAsync();
        }

        public async Task DeleteFileTypesByLibrary(int libid)
        {
            var currentLibrary = await _fileLibraryRepository.FindAsync(libid);
            var fileTypeIds = await GetLibraryFileTypeIdsAsync(libid);
            var fileTypesToRemove = currentLibrary.FileTypes
                .Where(_ => fileTypeIds.Any(__ => __ == _.FileTypeId))
                .Select(_ => _.FileTypeId)
                .ToList();
            await _fileLibraryRepository.RemoveLibraryFileTypesAsync(fileTypesToRemove, currentLibrary.Id);
        }

        public async Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId)
        {
            return await _fileLibraryRepository.GetLibraryFileTypeIdsAsync(libraryId);
        }

        public async Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId)
        {
            return await _fileRepository.GetFileTypeIdsInUseByLibraryAsync(libraryId);
        }

        public async Task<ICollection<FileType>> GetAllFileTypesAsync()
        {
            return await _fileTypeService.GetAllAsync();
        }

        public async Task<ICollection<int>> GetAllFileTypeIdsAsync()
        {
            return await _fileTypeService.GetAllIdsAsync();
        }

        public async Task<FileType> GetFileTypeByIdAsync(int id)
        {
            var types = await GetAllFileTypesAsync();
            return types.FirstOrDefault(_ => _.Id == id);
        }

        public async Task<List<File>> GetFileLibraryFilesAsync(int id)
        {
            return await _fileRepository.GetFileLibraryFilesAsync(id);
        }

        public async Task<List<FileLibrary>> GetFileLibrariesBySectionAsync(int sectionId)
        {
            return await _fileLibraryRepository.GetFileLibrariesBySectionIdAsync(sectionId);
        }

        public async Task<ICollection<FileType>> GetFileLibrariesFileTypesAsync(int libraryId)
        {
            return await _fileTypeService.GetTypesByLibraryIdsAsync(libraryId);
        }
    }
}
