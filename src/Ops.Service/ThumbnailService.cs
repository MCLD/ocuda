using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Service
{
    public class ThumbnailService : IThumbnailService
    {
        private readonly ILogger<ThumbnailService> _logger;
        private readonly IFileRepository _fileRepository;
        private readonly IThumbnailRepository _thumbnailRepository;
        private readonly IPathResolverService _pathResolver;

        public ThumbnailService(ILogger<ThumbnailService>  logger,
            IFileRepository fileRepository,
            IThumbnailRepository thumbnailRepository,
            IPathResolverService pathResolver)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _thumbnailRepository = thumbnailRepository
                ?? throw new ArgumentNullException(nameof(thumbnailRepository));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public string GetUrl(Thumbnail thumbnail)
        {
            var extension = System.IO.Path.GetExtension(thumbnail.Name);
            var url = _pathResolver.GetPublicContentUrl("thumbnails",
                $"file{thumbnail.FileId}",
                $"thumbnail{thumbnail.Id}{extension}");
            return url;
        }

        public async Task CreateThumbnailsAsync(
            ICollection<Thumbnail> thumbnails, ICollection<IFormFile> thumbnailFiles)
        {
            foreach (var thumbnail in thumbnails)
            {
                var fileData = thumbnailFiles.Where(_ => _.FileName == thumbnail.Name).Single();
                await WriteThumbnailAsync(thumbnail, fileData);
            }
        }

        private async Task WriteThumbnailAsync(Thumbnail thumbnail, IFormFile fileData)
        {
            string filePath = GetFilePath(thumbnail);
            byte[] fileBytes = IFormFileHelper.GetFileBytes(fileData);

            _logger.LogInformation($"Writing Thumbnail: {filePath}");

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }

        private string GetFilePath(Thumbnail thumbnail)
        {
            var extension = System.IO.Path.GetExtension(thumbnail.Name);
            return _pathResolver.GetPublicContentFilePath($"thumbnail{thumbnail.Id}{extension}",
                "thumbnails",
                $"file{thumbnail.FileId}");
        }

        public async Task DeleteThumbnailsAsync(IEnumerable<Thumbnail> thumbnailsToRemove)
        {
            foreach (var thumbnail in thumbnailsToRemove)
            {
                var filePath = GetFilePath(thumbnail);

                if (System.IO.File.Exists(filePath))
                {
                    _logger.LogInformation($"Deleting thumbnail: {filePath}");
                    System.IO.File.Delete(filePath);
                }

                _thumbnailRepository.Remove(thumbnail.Id);
                await _thumbnailRepository.SaveAsync();
            }
        }

        public async Task ValidateThumbnailAsync(Thumbnail thumbnail)
        {
            // TODO Update Validation

            var message = string.Empty;

            var file = await _fileRepository.FindAsync(thumbnail.FileId);

            if (file == null)
            {
                message = $"FileId '{thumbnail.FileId}' is not a valid file.";
                _logger.LogWarning(message, thumbnail.FileId);
                throw new OcudaException(message);
            }
        }
    }
}
