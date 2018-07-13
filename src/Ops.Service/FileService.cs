using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IFileRepository _fileRepository;
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly PathResolverService _pathResolver;

        public FileService(ILogger<FileService> logger,
            IFileRepository fileRepository,
            InsertSampleDataService insertSampleDataService,
            PathResolverService pathResolver)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
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
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = currentUserId;

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            file = WritePrivateFile(file, fileData, false);
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

        private File WritePrivateFile(File file, byte[] fileData, bool isEdit)
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

            //TODO refactor to WriteAllBytesAsync
            System.IO.File.WriteAllBytes(filePath, fileData);
            return file;
        }

        public async Task<File> EditPrivateFileAsync(File file, byte[] fileData = null)
        {
            var currentFile = await _fileRepository.FindAsync(file.Id);
            currentFile.Name = file.Name;
            currentFile.Description = file.Description;
            currentFile.CategoryId = file.CategoryId;
            currentFile.IsFeatured = file.IsFeatured;

            string filePath = GetPrivateFilePath(currentFile);

            if (fileData != null)
            {
                if(System.IO.File.Exists(filePath))
                {
                    _logger.LogInformation($"Editing File (Delete): {filePath}");
                    System.IO.File.Delete(filePath);
                }

                currentFile.Extension = file.Extension;
                currentFile.Icon = file.Icon;

                var newFile = WritePrivateFile(currentFile, fileData, true);
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
    }
}
