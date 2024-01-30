using System;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IImageService
    {
        public string BgColor { get; set; }
        public bool Crop { get; set; }
        public CropType CropType { get; set; }
        public int? CropXFocalPoint { get; set; }
        public int? CropYFocalPoint { get; set; }
        public bool DisableWebCall { get; set; }
        public bool Fit { get; set; }
        public Format Format { get; set; }
        public int? Height { get; set; }
        public HighDpi HighDpi { get; set; }
        public Quality Quality { get; set; }
        public bool TrimBorder { get; set; }
        public string Username { get; set; }
        public int? Width { get; set; }

        public Task<OptimizedImageResult> OptimizeAsync(Uri imageUri);

        public Task<OptimizedImageResult> OptimizeAsync(string imagePath);

        public Task<OptimizedImageResult> OptimizeAsync(IFormFile formFile);

        public (string extension, byte[] imageBytes) ConvertFromBase64(string imageBase64, bool profileImage = false);

        public string ConvertToBase64(byte[] imageBytes);

        public string GetExtension(byte[] imageBytes);
    }
}
