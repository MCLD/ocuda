using System;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.NavBannerViewModels;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class NavBannerController : BaseController<NavBannerController>
    {
        private readonly IImageService _imageService;
        private readonly ILanguageService _languageService;
        private readonly INavBannerService _navBannerService;

        public static string Name { get { return "NavBanner"; } }
        public static string Area { get { return "SiteManagement"; } }

        public NavBannerController(ServiceFacades.Controller<NavBannerController> context,
            IImageService imageService,
            ILanguageService languageService,
            INavBannerService navBannerService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerService);
            _imageService = imageService;
            _languageService = languageService;
            _navBannerService = navBannerService;
        }

        [HttpGet]
        [Route("{action}/{id}")]
        public async Task<IActionResult> Detail(int id, string language)
        {
            try
            {
                var navBanner = await _navBannerService.GetByIdAsync(id);

                var languages = await _languageService.GetActiveAsync();

                var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

                var navBannerImage = await _navBannerService.GetImageByNavBannerIdAsync(id);

                var viewModel = new DetailViewModel
                {
                    Name = navBanner.Name,
                    LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                    PageLayoutId = await _navBannerService.GetPageLayoutIdForNavBannerAsync(navBanner.Id),
                    NavBannerId = id,
                    NavBannerImage = navBannerImage
                };


                return View(viewModel);
            }
            catch (OcudaException oex)
            {
                Console.WriteLine(oex.Message);
                return NotFound();
            }
        }

        [HttpPost]
        [Route("{action}/{id}")]
        public async Task<IActionResult> Detail(DetailViewModel viewModel)
        {

            if (viewModel == null)
            {
                return View(viewModel);
            }

            if (viewModel.Language == null)
            {
                var languages = await _languageService.GetActiveAsync();

                viewModel.Language = languages.Single(_ => _.IsDefault);
            }

            var navBanner = await _navBannerService.GetByIdAsync(viewModel.NavBannerId);

            if (viewModel.Image != null)
            {
                OptimizedImageResult optimized;
                byte[] imageBytes;

                try
                {
                    optimized = await _imageService.OptimizeAsync(viewModel.Image);
                    imageBytes = optimized.File;
                    // get an approved filename with path
                    var filename = await _navBannerService.GetUploadImageFilePathAsync(viewModel.Language.Name,
                        viewModel.Image.FileName);

                    // copy file
                    await System.IO.File.WriteAllBytesAsync(filename, imageBytes);

                    var navBannerImage = await _navBannerService.GetImageByNavBannerIdAsync(viewModel.NavBannerId)
                        ?? new NavBannerImage();

                    var navBannerImagePath = _navBannerService.GetImageAssetPath(viewModel.Image.FileName, viewModel.Language.Name);

                    navBannerImage.NavBannerId = navBanner.Id;
                    navBannerImage.ImageAltText = viewModel.NavBannerImage.ImageAltText;
                    navBannerImage.LanguageId = viewModel.Language.Id;
                    navBannerImage.ImagePath = navBannerImagePath;

                    await _navBannerService.AddImageAsync(navBannerImage);
                }
                catch (ParameterException pex)
                {
                    ModelState.AddModelError("ItemImage",
                        $"Error optimizing uploaded image: {pex.Message}");
                }

                // Pick up here... save image path/alt text to a database entry
                
            }

            return RedirectToAction(nameof(PagesController.LayoutDetail), PagesController.Name, new { id = viewModel.PageLayoutId });
        }
    }
}
