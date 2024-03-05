using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
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
        [Route("{action}")]
        public async Task<IActionResult> BannerImage(int navBannerId, string languageName)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var navBannerImage = await _navBannerService.GetImageByNavBannerIdAsync(navBannerId);

            var basePath = await _navBannerService.GetFullImageDirectoryPath(languageName);

            var bannerImagePath = Path.Combine(
                promBasePath,
                basePath,
                Path.GetFileName(navBannerImage.ImagePath));

            if (!System.IO.File.Exists(bannerImagePath))
            {
                return StatusCode(404);
            }
            else
            {
                new FileExtensionContentTypeProvider()
                    .TryGetContentType(bannerImagePath, out string fileType);

                return PhysicalFile(bannerImagePath, fileType
                    ?? System.Net.Mime.MediaTypeNames.Application.Octet);
            }

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

                var navBannerLinks = await _navBannerService.GetLinksByNavBannerIdAsync(navBanner.Id, selectedLanguage.Id);

                var viewModel = new DetailViewModel
                {
                    Name = navBanner.Name,
                    LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                    PageLayoutId = await _navBannerService.GetPageLayoutIdForNavBannerAsync(navBanner.Id),
                    NavBannerId = id,
                    NavBannerImage = navBannerImage,
                    Links = navBannerLinks.ToArray(),
                    Language = selectedLanguage
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
                
            }

            if (viewModel.Links != null)
            {
                /*
                 * query DB for existing navBannerLinks associated with navBanner, then
                 * update them if they exist instead of creating new links
                 */

                var navBannerLinks = await _navBannerService.GetLinksByNavBannerIdAsync(navBanner.Id, viewModel.Language.Id)
                    ?? new List<NavBannerLink>();

                if (navBannerLinks.Count == 0)
                {
                    for (int i = 0; i < viewModel.Links.Length; i++)
                    {
                        navBannerLinks.Add(new NavBannerLink
                        {
                            Text = new NavBannerLinkText
                            {
                                Text = viewModel.Links[i].Text.Text,
                                LanguageId = viewModel.Language.Id,
                                NavBannerLink = navBannerLinks[i],
                                Link = viewModel.Links[i].Text.Link
                            },
                            Icon = viewModel.Links[i].Icon,
                            NavBannerId = navBanner.Id,
                            Order = i
                        });
                    }
                }

                await _navBannerService.AddLinksAsync(navBannerLinks);
            }

            return RedirectToAction(nameof(PagesController.LayoutDetail), PagesController.Name, new { id = viewModel.PageLayoutId });
        }
    }
}
