using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageOptimizer;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class ImageOptimizerController : BaseController<ImageOptimizerController>
    {
        public static readonly string Name = "ImageOptimizer";
        private readonly IImageService _imageService;
        private readonly IPermissionGroupService _permissionGroupService;

        public ImageOptimizerController(
            IImageService imageService,
            IPermissionGroupService permissionGroupService,
            ServiceFacades.Controller<ImageOptimizerController> context)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(imageService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _imageService = imageService;
            _permissionGroupService = permissionGroupService;
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
            if (!await HasPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            if (viewModel == null || viewModel.FormFile == null)
            {
                _logger.LogError("No uploaded image supplied.");
                return StatusCode(StatusCodes.Status404NotFound, "No uploaded image supplied.");
            }

            if (!_imageService.IsOptimizeConfigured())
            {
                _logger.LogError("Attempt to optimize image with optimization not configured.");
                return StatusCode(StatusCodes.Status403Forbidden,
                    "Image optimization not configured.");
            }

            try
            {
                _imageService.FormatMimeType = viewModel.TargetFormat;

                await using var memoryStream = new MemoryStream();
                await viewModel.FormFile.CopyToAsync(memoryStream);

                var optimized = await _imageService.OptimizeAsync(
                    memoryStream.ToArray(),
                    viewModel.FormFile.FileName);

                var filename = FileHelper.MakeValidFilename(Path.ChangeExtension(
                     viewModel.FormFile.FileName,
                     optimized.ImageInformation.Extension));

                return File(optimized.ImageBytes, optimized.ImageInformation.MimeType, filename);
            }
            catch (OcudaException oex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, oex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> OptimizeFromBase64([FromBody] string imageBase64)
        {
            if (!await HasPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            if (!_imageService.IsOptimizeConfigured())
            {
                _logger.LogError("Attempt to optimize image with optimization not configured.");
                return StatusCode(StatusCodes.Status403Forbidden,
                    "Image optimization not configured.");
            }

            try
            {
                var imageWithInfo = await _imageService.ConvertFromBase64Async(imageBase64);

                string filename = Path.GetTempFileName() + imageWithInfo.ImageInformation.Extension;

                var optimized = await _imageService.OptimizeAsync(
                    imageWithInfo.ImageBytes,
                    filename);

                return File(
                    optimized.ImageBytes,
                    optimized.ImageInformation.MimeType,
                    Path.ChangeExtension(filename, optimized.ImageInformation.Extension));
            }
            catch (OcudaException oex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, oex.Message);
            }
        }

        private async Task<bool> HasPermissionAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ImageOptimizer);
        }
    }
}
