using System;
using System.IO;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageOptimizer;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class ImageOptimizerController : BaseController<ImageOptimizerController>
    {
        private readonly IImageService _imageOptimizerService;
        public static readonly string Name = "ImageOptimizer";

        public ImageOptimizerController(
            IImageService imageOptimizerService,
            ServiceFacades.Controller<ImageOptimizerController> context) : base(context)
        {
            _imageOptimizerService = imageOptimizerService ?? throw new ArgumentNullException(nameof(imageOptimizerService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ImageOptimizerViewModel();

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Index(ImageOptimizerViewModel viewModel)
        {
            if (viewModel == null || viewModel.FormFile == null)
            {
                _logger.LogError("No uploaded image supplied.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "No uploaded image supplied.");
            }

            OptimizedImageResult optimized;

            string filename = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName())
                + Path.GetExtension(viewModel.FormFile.FileName));

            try
            {
                using var stream = new FileStream(filename, FileMode.Create);
                await viewModel.FormFile.CopyToAsync(stream);
                stream.Close();

                optimized = await _imageOptimizerService.OptimizeAsync(filename);
                _logger.LogInformation("Image optimization took {ElapsedSeconds}s",
                    optimized.ElapsedSeconds);
            }
            catch (ParameterException pex)
            {
                _logger.LogError("Error with image submission: ", pex.Message);
                return null;

            }
            finally
            {
                System.IO.File.Delete(filename);
            }

            if (optimized != null)
            {
                if (optimized.Status == Status.Success)
                {
                    string newFilename = _imageOptimizerService.Format == Format.Auto
                        ? viewModel.FormFile.FileName
                        : Path.GetFileNameWithoutExtension(viewModel.FormFile.FileName)
                            + GetExtension(_imageOptimizerService.Format);
                    var provider = new FileExtensionContentTypeProvider();
                    provider.TryGetContentType(newFilename, out var contentType);
                    return File(optimized.File,
                        contentType ?? "application/octet-stream",
                        newFilename);
                }
                else
                {
                    _logger.LogError("Problem optimizing image: {ErrorStatus} {ErrorMessage}",
                        optimized.Status,
                        optimized.StatusMessage);
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        $"{optimized.Status}: {optimized.StatusMessage}");
                }
            }
            else
            {
                _logger.LogError("No optimized image returned.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "No optimized image returned.");
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> OptimizeBase64(string imageBase64)
        {

        }

        private static string GetExtension(Format format) => format switch
        {
            Format.Png => ".png",
            Format.Jpeg => ".jpg",
            Format.WebM => ".webm",
            Format.H264 => ".h264",
            _ => string.Empty,
        };
    }
}
