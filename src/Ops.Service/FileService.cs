using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public FileService(InsertSampleDataService insertSampleDataService,
            IFileRepository fileRepository)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
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

        public async Task<File> CreateAsync(File file)
        {
            // TODO Save file

            file.CreatedAt = DateTime.Now;
            // TODO Set CreatedBy Id
            file.CreatedBy = 1;

            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();
            return file;
        }

        public async Task<File> EditAsync(File file)
        {
            var currentFile = await _fileRepository.FindAsync(file.Id);
            currentFile.Name = file.Name;
            currentFile.Description = file.Description;
            currentFile.CategoryId = file.CategoryId;
            currentFile.FilePath = file.FilePath;
            currentFile.Icon = file.Icon;
            currentFile.IsFeatured = file.IsFeatured;

            _fileRepository.Update(currentFile);
            await _fileRepository.SaveAsync();
            return file;
        }

        public async Task DeleteAsync(int id)
        {
            _fileRepository.Remove(id);
            await _fileRepository.SaveAsync();
        }  
    }
}
