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
        private readonly ICategoryRepository _categoryRepository;

        public FileService(InsertSampleDataService insertSampleDataService,
            IFileRepository fileRepository,
            ICategoryRepository categoryRepository)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _categoryRepository = categoryRepository 
                ?? throw new ArgumentNullException(nameof(categoryRepository));
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

        public async Task<ICollection<Category>> GetFileCategoriesAsync()
        {
            // TODO repository/database
            return await _categoryRepository.ToListAsync(_ => _.Name);
        }

        public async Task<Category> GetFileCategoryByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<int> GetFileCategoryCountAsync()
        {
            return await _categoryRepository.CountAsync();
        }

        public async Task<Category> CreateFileCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.Now;
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return category;
        }

        public async Task<Category> EditFileCategoryAsync(Category category)
        {
            // TODO fix edit logic
            // get existing item and update properties that changed
            // call edit method on existing category
            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();
            return category;
        }

        public async Task DeleteFileCategoryAsync(int id)
        {
            _categoryRepository.Remove(id);
            await _categoryRepository.SaveAsync();
        }
    }
}
