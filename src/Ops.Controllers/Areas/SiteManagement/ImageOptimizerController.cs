using System;
using System.IO;
using System.Net;
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
        public static readonly string Name = "ImageOptimizer";
        private readonly IImageService _imageService;

        public ImageOptimizerController(
            IImageService imageService,
            ServiceFacades.Controller<ImageOptimizerController> context) : base(context)
        {
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public IActionResult Index()
        {
            return View(new ImageOptimizerViewModel());
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

            try
            {
                _imageService.Format = viewModel.TargetFormat;

                optimized = await _imageService.OptimizeAsync(viewModel.FormFile);

                string newFilename = Path.GetFileNameWithoutExtension(viewModel.FormFile.FileName)
                    + _imageService.GetExtension(optimized.File);

                var provider = new FileExtensionContentTypeProvider();
                provider.TryGetContentType(newFilename, out var contentType);
                return File(optimized.File,
                    contentType ?? "application/octet-stream",
                    newFilename);
            }
            catch (ParameterException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        pex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> OptimizeFromBase64([FromBody] string imageBase64)
        {
            try
            {
                var (extension, imageBytes) = _imageService.ConvertFromBase64(imageBase64);

                string filePath = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName())
                + extension);

                System.IO.File.WriteAllBytes(filePath, imageBytes);

                var optimized = await _imageService.OptimizeAsync(filePath);

                var optimizedBase64 = _imageService.ConvertToBase64(optimized.File);

                return new JsonResult(optimizedBase64);
            }
            catch (ParameterException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        pex.Message);
            }
        }
    }
}