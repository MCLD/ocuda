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
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<FileService> _logger;

        public FileService(InsertSampleDataService insertSampleDataService,
            IFileRepository fileRepository,
            ILogger<FileService> logger)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
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

        public async Task<File> CreateAsync(File file, byte[] fileData)
        {
            file.CreatedAt = DateTime.Now;
            file.CreatedBy = 1; // TODO Set CreatedBy Id

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();

            file.FilePath = WriteFileData(file, fileData, false);
            _fileRepository.Update(file);
            await _fileRepository.SaveAsync();

            return file;
        }

        private string WriteFileData(File file, byte[] fileData, bool isEdit)
        {
            var sectionId = file.SectionId;
            string fileName = $"file{file.Id}{file.Extension}";
            string fullFilePath = GetFilePath(fileName, sectionId);

            if(isEdit)
            {
                _logger.LogInformation($"Editing File (Create): {fullFilePath}");
            }
            else
            {
                _logger.LogInformation($"Writing file: {fullFilePath}");
            }
            
            System.IO.File.WriteAllBytes(fullFilePath, fileData);
            return GetUrlPath(fileName, sectionId);
        }

        private string GetFilePath(string fileName, int sectionId)
        {
            //TODO path resolution
            string contentDir = $"Shared\\Content\\BlogFiles\\{ sectionId }";

            if (!System.IO.Directory.Exists(contentDir))
            {
                System.IO.Directory.CreateDirectory(contentDir);
            }

            return System.IO.Path.Combine(contentDir, fileName);
        }

        private string GetUrlPath(string fileName, int sectionId)
        {
            //TODO path resolution
            return $"Shared\\Content\\BlogFiles\\{ sectionId }\\{ fileName }";
        }

        public async Task<File> EditAsync(File file, byte[] fileData = null)
        {
            var currentFile = await _fileRepository.FindAsync(file.Id);
            currentFile.Name = file.Name;
            currentFile.Description = file.Description;
            currentFile.CategoryId = file.CategoryId;
            currentFile.IsFeatured = file.IsFeatured;

            if (fileData != null)
            {
                if(System.IO.File.Exists(currentFile.FilePath))
                {
                    _logger.LogInformation($"Editing File (Delete): {currentFile.FilePath}");
                    System.IO.File.Delete(currentFile.FilePath);
                }

                currentFile.FilePath = WriteFileData(file, fileData, true);
                currentFile.Extension = System.IO.Path.GetExtension(currentFile.FilePath);
                currentFile.Icon = file.Icon;
            }

            _fileRepository.Update(currentFile);
            await _fileRepository.SaveAsync();
            return currentFile;
        }

        public async Task DeleteAsync(int id)
        {
            var file = await _fileRepository.FindAsync(id);

            if (System.IO.File.Exists(file.FilePath))
            {
                _logger.LogInformation($"Deleting file: {file.FilePath}");
                System.IO.File.Delete(file.FilePath);
            }

            _fileRepository.Remove(id);
            await _fileRepository.SaveAsync();
        }
    }
}
