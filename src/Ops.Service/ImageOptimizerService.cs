using System;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service
{
    public class ImageOptimizerService : IImageOptimizerService
    {
        private readonly Client _client;
        private readonly IConfiguration _config;

        public ImageOptimizerService(Client client, IConfiguration config)
        {
            _client = client;
            _config = config;
            _client.Username = _config[Configuration.OpsImageOptimizerUsername];
        }

        public string BgColor { get => _client.BgColor; set => _client.BgColor = value; }
        public bool Crop { get => _client.Crop; set => _client.Crop = value; }
        public CropType CropType { get => _client.CropType; set => _client.CropType = value; }
        public int? CropXFocalPoint { get => _client.CropXFocalPoint; set => _client.CropXFocalPoint = value; }
        public int? CropYFocalPoint { get => _client.CropXFocalPoint; set => _client.CropYFocalPoint = value; }
        public bool DisableWebCall { get => _client.DisableWebCall; set => _client.DisableWebCall = value; }
        public bool Fit { get => _client.Fit; set => _client.Fit = value; }
        public Format Format { get => _client.Format; set => _client.Format = value; }
        public int? Height { get => _client.Height; set => _client.Height = value; }
        public HighDpi HighDpi { get => _client.HighDpi; set => _client.HighDpi = value; }
        public Quality Quality { get => _client.Quality; set => _client.Quality = value; }
        public bool TrimBorder { get => _client.TrimBorder; set => _client.TrimBorder = value; }
        public string Username { get => _client.Username; set => _client.Username = value; }
        public int? Width { get => _client.Width; set => _client.Width = value; }

        public async Task<OptimizedImageResult> OptimizeAsync(Uri imageUri)
        {
            return await _client.OptimizeAsync(imageUri);
        }

        public async Task<OptimizedImageResult> OptimizeAsync(string imagePath)
        {
            return await _client.OptimizeAsync(imagePath);
        }
    }
}
