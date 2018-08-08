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
        Task CreateThumbnailsAsync(ICollection<Thumbnail> thumbnails, ICollection<IFormFile> thumbnailFiles);
        Task DeleteThumbnailsAsync(IEnumerable<Thumbnail> thumbnailsToRemove);
        string GetUrl(Thumbnail thumbnail);
    }
}
