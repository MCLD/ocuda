using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;
using Serilog.Context;

namespace Ocuda.Ops.Service
{
    public class ImageService : BaseService<ImageService>, IImageService
    {
        private const string ConfigKey = nameof(Utility.Keys.Configuration.OpsImageOptimizerUsername);
        private const string MimeMediaTypeNamesVideoH264 = "video/h264";
        private const string MimeMediaTypeNamesVideoWebm = "video/webm";
        private static readonly string[] ValidImageTypes = [
            System.Net.Mime.MediaTypeNames.Image.Jpeg,
            System.Net.Mime.MediaTypeNames.Image.Png,
            ];

        private readonly Client client;
        private readonly IImageTools imageTools;

        public ImageService(
            Client imageOptimApiClient,
            IConfiguration config,
            ILogger<ImageService> logger,
            IHttpContextAccessor httpContextAccessor,
            IImageTools imageTools)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(imageOptimApiClient);
            ArgumentNullException.ThrowIfNull(imageTools);

            client = imageOptimApiClient;
            this.imageTools = imageTools;

            client.Username = config[Utility.Keys.Configuration.OpsImageOptimizerUsername];
        }

        public string FormatMimeType
        {
            get
            {
                return client.Format switch
                {
                    Format.H264 => MimeMediaTypeNamesVideoH264,
                    Format.Jpeg => System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    Format.Png => System.Net.Mime.MediaTypeNames.Image.Png,
                    Format.WebM => MimeMediaTypeNamesVideoWebm,
                    _ => null,
                };
            }

            set
            {
                client.Format = value switch
                {
                    MimeMediaTypeNamesVideoH264 => Format.H264,
                    MimeMediaTypeNamesVideoWebm => Format.WebM,
                    System.Net.Mime.MediaTypeNames.Image.Jpeg => Format.Jpeg,
                    System.Net.Mime.MediaTypeNames.Image.Png => Format.Png,
                    _ => Format.Auto,
                };
            }
        }

        public async Task<ImageWithInformation> ConvertFromBase64Async(
            string imageBase64)
        {
            return await ConvertFromBase64Async(imageBase64, false);
        }

        public async Task<ImageWithInformation> ConvertFromBase64Async(
            string imageBase64,
            bool isSquare)
        {
            var result = new ImageWithInformation
            {
                ImageBytes = Convert.FromBase64String(imageBase64),
            };

            result.ImageInformation = await GetInformationAsync(result.ImageBytes);

            if (isSquare && result.ImageInformation.Height != result.ImageInformation.Width)
            {
                throw new OcudaException("Image must be square (equal width and height).");
            }

            var validFileType = ValidImageTypes.Contains(result.ImageInformation.MimeType);

            return !validFileType
                ? throw new OcudaException(
                    "Invalid image format, please upload a JPEG or PNG picture")
                : result;
        }

        public async Task<string> ConvertToBase64Async(byte[] imageBytes)
        {
            var imageInfo = await GetInformationAsync(imageBytes);
            var metaData = $"data:{imageInfo.MimeType};base64,";
            var base64 = Convert.ToBase64String(imageBytes);
            return metaData + base64;
        }

        public async Task<ImageInformation> GetInformationAsync(byte[] imageBytes)
        {
            return await imageTools.GetInformationAsync(imageBytes);
        }

        public bool IsOptimizeConfigured()
        {
            return !string.IsNullOrEmpty(client.Username);
        }

        public async Task<ImageWithInformation> OptimizeAsync(byte[] imageBytes, string filename)
        {
            ArgumentNullException.ThrowIfNull(filename);

            if (string.IsNullOrEmpty(client?.Username))
            {
                throw new OcudaConfigurationException(
                    $"Unable to optimize image, missing configuration: {ConfigKey}");
            }

            OptimizedImageResult optimized;

            try
            {
                optimized = imageBytes == null
                    ? await OptimizeAsyncInternal(
                        await File.ReadAllBytesAsync(filename),
                        Path.GetFileName(filename))
                    : await OptimizeAsyncInternal(imageBytes, Path.GetFileName(filename));
            }
            catch (ParameterException pex)
            {
                _logger.LogError("Error with image submission: {ErrorMessage}", pex.Message);
                return null;
            }

            return optimized.Status != Status.Success
                ? throw new OcudaException(
                    $"Unable to optimize image: {optimized.Status} - {optimized.StatusMessage}")
                : new ImageWithInformation
                {
                    ImageBytes = optimized.File,
                    ImageInformation = await GetInformationAsync(optimized.File),
                };
        }

        private async Task<OptimizedImageResult> OptimizeAsyncInternal(
            byte[] imageBytes,
            string filename)
        {
            if (string.IsNullOrEmpty(client?.Username))
            {
                throw new OcudaConfigurationException(
                    $"Unable to optimize image, missing configuration: {ConfigKey}");
            }

            var sourceInfo = await GetInformationAsync(imageBytes);

            using (LogContext.PushProperty("ImageName", filename))
            {
                if (string.IsNullOrEmpty(FormatMimeType))
                {
                    OptimizedImageResult optimizedJpeg = null;
                    FormatMimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    try
                    {
                        optimizedJpeg = await client.OptimizeAsync(imageBytes, filename);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "An error occurred optimizing {SourceFormat} file {Filename} to {Format} image: {ErrorMessage}",
                            sourceInfo.MimeType,
                            filename,
                            FormatMimeType,
                            ex.Message);
                        throw;
                    }

                    OptimizedImageResult optimizedPng = null;
                    FormatMimeType = System.Net.Mime.MediaTypeNames.Image.Png;
                    try
                    {
                        optimizedPng = await client.OptimizeAsync(imageBytes, filename);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "An error occurred optimizing {SourceFormat} file {Filename} to {Format} image: {ErrorMessage}",
                            sourceInfo.MimeType,
                            filename,
                            FormatMimeType,
                            ex.Message);
                        throw;
                    }

                    var jpegInfo = await imageTools.GetInformationAsync(optimizedJpeg.File);
                    var pngInfo = await imageTools.GetInformationAsync(optimizedPng.File);

                    _logger.LogInformation(
                        "Optimized {SourceFormat} file {Filename} from {OriginalSize:n0} to JPEG {JpegType} size {JpegSize:n0} in {ElapsedJpeg:n2} s, PNG {PngType} size {PngSize:n0} in {ElapsedPng:n2} s",
                        sourceInfo.MimeType,
                        filename,
                        optimizedJpeg.OriginalSize,
                        jpegInfo.MimeType,
                        optimizedJpeg.File.Length,
                        optimizedJpeg.ElapsedSeconds,
                        pngInfo.MimeType,
                        optimizedPng.File.Length,
                        optimizedPng.ElapsedSeconds);

                    FormatMimeType = null;

                    return optimizedJpeg.File.Length < optimizedPng.File.Length
                        ? optimizedJpeg
                        : optimizedPng;
                }
                else
                {
                    OptimizedImageResult optimized = null;
                    try
                    {
                        optimized = imageBytes != null
                            ? await client.OptimizeAsync(imageBytes, filename)
                            : await client.OptimizeAsync(filename);
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(
                            ex,
                            "An error occurred optimizing {SourceFormat} file {Filename} to {Format} image: {ErrorMessage}",
                            sourceInfo.MimeType,
                            filename,
                            FormatMimeType,
                            ex.Message);
                        throw;
                    }

                    var imageInfo = await imageTools.GetInformationAsync(optimized.File);

                    _logger.LogInformation(
                        "Optimized {SourceFormat} file {Filename} from {OriginalSize:n0} requested {MimeType} output {FileType} size {JpegSize:n0} in {ElapsedJpeg:n2} s",
                        sourceInfo.MimeType,
                        filename,
                        optimized.OriginalSize,
                        FormatMimeType,
                        imageInfo.MimeType,
                        optimized.File.Length,
                        optimized.ElapsedSeconds);

                    return optimized;
                }
            }
        }
    }
}
