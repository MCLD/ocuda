using System;
using System.IO;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageOptimizer;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
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
            ServiceFacades.Controller<ImageOptimizerController> context) : base(context)
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
            catch (OcudaConfigurationException)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> OptimizeFromBase64([FromBody] string imageBase64)
        {
            if (!await HasPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var (extension, imageBytes) = _imageService.ConvertFromBase64(imageBase64);

                string filePath = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetTempFileName())
                + extension);

                System.IO.File.WriteAllBytes(filePath, imageBytes);

                OptimizedImageResult optimized = null;
                try
                {
                    optimized = await _imageService.OptimizeAsync(filePath);
                }
                catch (OcudaConfigurationException)
                {
                    return StatusCode(500);
                }

                var optimizedBase64 = _imageService.ConvertToBase64(optimized.File);

                return new JsonResult(optimizedBase64);
            }
            catch (ParameterException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        pex.Message);
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