using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using SixLabors.ImageSharp;

namespace Ocuda.Ops.Service
{
    public class ImageService : BaseService<ImageService>, IImageService
    {
        private static readonly string[] ValidImageTypes = { ".jpg", ".png" };
        private readonly Client _client;
        private readonly IConfiguration _config;

        public ImageService(
            Client client,
            IConfiguration config,
            ILogger<ImageService> logger,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(config);

            _client = client;
            _config = config;

            _client.Username = _config[Utility.Keys.Configuration.OpsImageOptimizerUsername];
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

        public (string extension, byte[] imageBytes) ConvertFromBase64(string imageBase64)
        {
            return ConvertFromBase64(imageBase64, false);
        }

        public (string extension, byte[] imageBytes) ConvertFromBase64(string imageBase64, bool profileImage)
        {
            byte[] imageBytes;

            string extension;

            try
            {
                imageBytes = Convert.FromBase64String(imageBase64);
                var imageInfo = Image.Identify(imageBytes);

                if (profileImage && imageInfo.Height != imageInfo.Width)
                {
                    throw new OcudaException("Profile picture must be square.");
                }

                var imageFormat = imageInfo.Metadata.DecodedImageFormat;

                var validFileType = false;
                foreach (var validExtension in ValidImageTypes)
                {
                    if (imageFormat.MimeTypes.Contains(MimeTypes.GetMimeType(validExtension)))
                    {
                        validFileType = true;
                        break;
                    }
                }

                if (!validFileType)
                {
                    throw new OcudaException("Invalid image format, please upload a JPEG or PNG picture");
                }
                extension = '.' + imageFormat.FileExtensions.First();
            }
            catch (UnknownImageFormatException uifex)
            {
                throw new OcudaException("Unknown image type, please upload a JPEG or PNG picture",
                    uifex);
            }

            return (extension, imageBytes);
        }

        public string ConvertToBase64(byte[] imageBytes)
        {
            try
            {
                var imageInfo = Image.Identify(imageBytes);
                var metaData = "data:" + imageInfo.Metadata.DecodedImageFormat.DefaultMimeType + ";base64,";
                var base64 = Convert.ToBase64String(imageBytes);
                return metaData + base64;
            }
            catch (UnknownImageFormatException uifex)
            {
                throw new OcudaException("Unknown image type, please upload a JPEG or PNG picture",
                    uifex);
            }
        }

        public string GetExtension(byte[] imageBytes)
        {
            try
            {
                var imageInfo = Image.Identify(imageBytes);
                return '.' + imageInfo.Metadata.DecodedImageFormat.FileExtensions.First();
            }
            catch (UnknownImageFormatException uifex)
            {
                throw new OcudaException("Unknown image type, please upload a JPEG or PNG picture",
                    uifex);
            }
        }

        public string GetMimeType(byte[] imageBytes)
        {
            try
            {
                var imageInfo = Image.Identify(imageBytes);
                return imageInfo.Metadata.DecodedImageFormat.DefaultMimeType;
            }
            catch (UnknownImageFormatException uifex)
            {
                throw new OcudaException("Unknown image type, please upload a JPEG or PNG picture",
                    uifex);
            }
        }

        public async Task<OptimizedImageResult> OptimizeAsync(Uri imageUri)
        {
            return await _client.OptimizeAsync(imageUri);
        }

        public async Task<OptimizedImageResult> OptimizeAsync(string imagePath)
        {
            return await _client.OptimizeAsync(imagePath);
        }

        public async Task<OptimizedImageResult> OptimizeAsync(IFormFile formFile)
        {
            ArgumentNullException.ThrowIfNull(formFile);

            OptimizedImageResult optimized;

            string filePath = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName())
                + Path.GetExtension(formFile.FileName));

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await formFile.CopyToAsync(stream);
                stream.Close();

                optimized = await OptimizeAsync(filePath);
                _logger.LogInformation("Image optimization took {ElapsedSeconds}s",
                    optimized.ElapsedSeconds);
            }
            catch (ParameterException pex)
            {
                _logger.LogError("Error with image submission: {ErrorMessage}", pex.Message);
                return null;
            }
            finally
            {
                File.Delete(filePath);
            }

            if (optimized?.Status != Status.Success)
            {
                _logger.LogError("Problem optimizing image, status {ErrorStatus}: {ErrorMessage}",
                        optimized.Status,
                        optimized.StatusMessage);
                throw new OcudaException($"The image {formFile.FileName} could not be optimized.");
            }

            return optimized;
        }
    }
}