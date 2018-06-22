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
            // TODO check edit logic
            // TODO edit saved file
            var currentFile = await _fileRepository.FindAsync(file.Id);
            currentFile.Description = file.Description;
            currentFile.Name = file.Name;

            _fileRepository.Update(file);
            await _fileRepository.SaveAsync();
            return file;
        }

        public async Task DeleteAsync(int id)
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
