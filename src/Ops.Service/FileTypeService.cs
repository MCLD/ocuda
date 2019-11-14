using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class FileTypeService : IFileTypeService
    {
        private readonly IFileTypeRepository _fileTypeRepository;

        public FileTypeService(IFileTypeRepository fileTypeRepository)
        {
            _fileTypeRepository = fileTypeRepository
                ?? throw new ArgumentNullException(nameof(fileTypeRepository));
        }

        public async Task<ICollection<FileType>> GetAllAsync()
        {
            return await _fileTypeRepository.ToListAsync(_ => _.Extension);
        }

        public ICollection<int> GetAllIds()
        {
            return _fileTypeRepository.GetAllIds();
        }

        public async Task<ICollection<FileType>> GetTypesByLibraryIdsAsync(int libId)
        {
            return await _fileTypeRepository.GetAllTypesByLibraryIdAsync(libId);
        }

        public async Task<ICollection<FileType>> GetAllExtensionsAsync()
        {
            return await _fileTypeRepository.GetAllExtensionsAsync();
        }

        public async Task<FileType> GetByExtensionAsync(string extension)
        {
            return await _fileTypeRepository.GetByExtensionAsync(extension);
        }

        public async Task<FileType> GetByIdAsync(int id)
        {
            return await _fileTypeRepository.FindAsync(id);
        }
    }
}
