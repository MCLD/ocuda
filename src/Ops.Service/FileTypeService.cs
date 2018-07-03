using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Service
{
    public class FileTypeService
    {
        private readonly IFileTypeRepository _fileTypeRepository;
        private readonly InsertSampleDataService _insertSampleDataService;

        public FileTypeService(IFileTypeRepository fileTypeRepository,
            InsertSampleDataService insertSampleDataService)
        {
            _fileTypeRepository = fileTypeRepository 
                ?? throw new ArgumentNullException(nameof(fileTypeRepository));
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
        }

        public async Task<FileType> GetByExtensionAsync(string extension)
        {
            return await _fileTypeRepository.GetByExtensionAsync(extension);
        }
    }
}
