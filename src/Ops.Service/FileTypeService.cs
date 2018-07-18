using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
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

        public async Task<FileType> GetByExtensionAsync(string extension)
        {
            return await _fileTypeRepository.GetByExtensionAsync(extension);
        }
    }
}
