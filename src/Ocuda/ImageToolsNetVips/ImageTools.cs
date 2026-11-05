using Microsoft.Extensions.Logging;
using MimeTypeCore;
using NetVips;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace ImageToolsNetVips
{
    public class ImageTools : IImageTools
    {
        private const string VipsForeignLoadJpegBuffer = nameof(VipsForeignLoadJpegBuffer);
        private const string VipsForeignLoadPngBuffer = nameof(VipsForeignLoadPngBuffer);

        private readonly ILogger<ImageTools> Logger;
        private readonly Dictionary<string, VOption> MimeTypeVOptions;

        public ImageTools(ILogger<ImageTools> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            Logger = logger;

            MimeTypeVOptions = new Dictionary<string, VOption>
            {
                {
                    System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    new VOption
                    {
                        {"Q", 84 }
                    }
                },
                {
                    System.Net.Mime.MediaTypeNames.Image.Png,
                    new VOption
                    {
                        {"compression", 9 }
                    }
                }
            };
        }

        private class StreamAndMetadata
        {
            public MemoryStream Stream { get; set; } = null!;
            public string MimeType { get; set; } = null!;
            public string Extension { get; set; } = null!;
        }

        private static StreamAndMetadata GetStreamMimeTypeExtension(MemoryStream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new OcudaException("Unable to access image data stream.");
            }

            if (!MimeTypeMap.TryGetMimeType(stream, out string? mimeType))
            {
                var loader = Image.FindLoadBuffer(stream.ToArray());
                mimeType = loader switch
                {
                    VipsForeignLoadJpegBuffer => System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    VipsForeignLoadPngBuffer => System.Net.Mime.MediaTypeNames.Image.Png,
                    _ => throw new OcudaException($"Unrecognized image type, decoder: {loader}."),
                };
            }

            if (string.IsNullOrEmpty(mimeType))
            {
                throw new OcudaException("Unrecognized image type.");
            }

            if (!MimeTypeMap.TryGetExtension(mimeType, out var extension))
            {
                throw new OcudaException($"Can't determine image file extension for: {mimeType}");
            }

            stream.Seek(0, SeekOrigin.Begin);

            return new StreamAndMetadata
            {
                Extension = extension,
                MimeType = mimeType,
                Stream = stream,
            };
        }

        public async Task<byte[]> GetCroppedAsync(
            byte[] imageBytes,
            int x,
            int y,
            int width,
            int height)
        {
            using var stream = new MemoryStream(imageBytes, false);
            var streamMetadata = GetStreamMimeTypeExtension(stream);
            byte[] result;

            string step = "loading image";
            try
            {
                using var image = Image.NewFromStream(
                    streamMetadata.Stream,
                    access: Enums.Access.Sequential);

                step = "cropping image";
                using var cropImage = image.Crop(x, y, width, height);

                step = "outputting cropped image";
                result = cropImage.WriteToBuffer(
                    streamMetadata.Extension,
                    MimeTypeVOptions[streamMetadata.MimeType]);
            }
            catch (VipsException vex)
            {
                throw new OcudaException($"Image crop failed while {step}: {vex.Message}", vex);
            }

            return await Task.FromResult(result);
        }

        public async Task<ImageInformation> GetInformationAsync(byte[] imageBytes)
        {
            using var stream = new MemoryStream(imageBytes, false);
            var streamMetadata = GetStreamMimeTypeExtension(stream);

            using var image = Image.NewFromStream(
                streamMetadata.Stream,
                access: Enums.Access.Sequential);

            return await Task.FromResult(new ImageInformation
            {
                Extension = streamMetadata.Extension,
                Height = image.Height,
                MimeType = streamMetadata.MimeType,
                Width = image.Width,
            });
        }

        public async Task<byte[]> GetLayeredImageAsync(IEnumerable<byte[]> layers, int x, int y)
        {
            if (layers == null || layers.Count() < 2)
            {
                throw new OcudaException("Need more than one image to layer.");
            }

            using var stream = new MemoryStream(layers.First(), false);
            var streamMetadata = GetStreamMimeTypeExtension(stream);
            byte[] result;
            var layerImages = new List<Image>(layers.Count() - 1);

            string step = "loading first (background) image";
            try
            {
                using var image = Image.NewFromStream(
                    streamMetadata.Stream,
                    access: Enums.Access.Sequential);

                var layerCounter = 2;
                foreach (var layer in layers.Skip(1))
                {
                    step = $"loading image {layerCounter++}";
                    using var layerImage = Image.NewFromBuffer(layer);
                    layerImages.Add(layerImage);
                }

                step = "creating composite image";
                using var composite = image.Composite(
                    [.. layerImages],
                    [.. Enumerable.Repeat(Enums.BlendMode.Over, layerImages.Count)],
                    [.. Enumerable.Repeat(x, layerImages.Count)],
                    [.. Enumerable.Repeat(y, layerImages.Count)]);

                step = "outputting composite image";
                result = composite.WriteToBuffer(
                    streamMetadata.Extension,
                    MimeTypeVOptions[streamMetadata.MimeType]);
            }
            catch (VipsException vex)
            {
                throw new OcudaException($"Image layering failed while {step}: {vex.Message}", vex);
            }

            return await Task.FromResult(result);
        }

        public async Task<byte[]> GetResizedAsync(byte[] imageBytes, int width, int height)
        {
            using var stream = new MemoryStream(imageBytes, false);
            var streamMetadata = GetStreamMimeTypeExtension(stream);
            byte[] result;

            string step = "loading image";
            try
            {
                using var image = Image.NewFromStream(
                    streamMetadata.Stream,
                    access: Enums.Access.Sequential);

                step = "resizing image";
                using var resizedImage = image.ThumbnailImage(width, height);

                step = "outputting resized image";
                result = resizedImage.WriteToBuffer(
                    streamMetadata.Extension,
                    MimeTypeVOptions[streamMetadata.MimeType]);
            }
            catch (VipsException vex)
            {
                throw new OcudaException($"Image resize failed while {step}: {vex.Message}", vex);
            }

            return await Task.FromResult(result);
        }
    }
}
