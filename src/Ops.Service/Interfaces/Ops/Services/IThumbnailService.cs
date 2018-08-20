using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IThumbnailService
    {
        Task CreateThumbnailFilesAsync(ICollection<Thumbnail> thumbnails, ICollection<IFormFile> thumbnailFiles);
        void DeleteThumbnailFiles(IEnumerable<Thumbnail> thumbnailsToRemove);
        string GetUrl(Thumbnail thumbnail);
    }
}
