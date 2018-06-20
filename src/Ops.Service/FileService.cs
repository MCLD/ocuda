using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Data.Ops;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class FileService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly FileRepository _fileRepository;

        public FileService(InsertSampleDataService insertSampleDataService,
            FileRepository fileRepository)
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
            var files = await _fileRepository.ToListAsync(_ => _.Name);
            if (files == null || files.Count == 0)
            {
                files = await _fileRepository.ToListAsync(_ => _.Name);
            }
            return files;
        }

        public async Task<File> GetFileByIdAsync(int id)
        {
            return await _fileRepository.FindAsync(id);
        }

        public async Task<File> CreateFileAsync(File file)
        {
            file.CreatedAt = DateTime.Now;
            await _fileRepository.AddAsync(file);
            await _fileRepository.SaveAsync();
            return file;
        }

        public async Task<File> EditFileAsync(File file)
        {
            // TODO fix edit logic
            // get existing post and update properties that changed
            // call edit method on existing post
            _fileRepository.Update(file);
            await _fileRepository.SaveAsync();
            return file;
        }

        public async Task DeleteFileAsync(int id)
        {
            _fileRepository.Remove(id);
            await _fileRepository.SaveAsync();
        }
        public IEnumerable<FileCategory> GetFileCategories()
        {
            // TODO repository/database
            return new List<FileCategory>
            {
                new FileCategory
                {
                    Id = 1,
                    Name = "File Category 1",
                },
                new FileCategory
                {
                    Id = 2,
                    Name = "File Category 2",
                },
                new FileCategory
                {
                    Id = 3,
                    Name = "File Category 3",
                },
            };
        }

        public async Task<FileCategory> CreateFileCategoryAsync(FileCategory fileCategory)
        {
            // TODO repository/database
            // call create method from repository
            return fileCategory;
        }

        public async Task<FileCategory> EditFileCategoryAsync(FileCategory fileCategory)
        {
            // TODO repository/database
            // get existing item and update properties that changed
            // call edit method on existing post
            return fileCategory;
        }

        public async Task DeleteFileCategoryAsync(int id)
        {
            // TODO repository/database
            // call delete method from repository
            throw new NotImplementedException();
        }
    }
}
