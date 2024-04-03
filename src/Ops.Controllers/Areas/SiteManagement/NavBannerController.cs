using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.NavBannerViewModels;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
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
        private readonly IPermissionGroupService _permissionGroupService;

        public static string Name { get { return "NavBanner"; } }
        public static string Area { get { return "SiteManagement"; } }

        public NavBannerController(ServiceFacades.Controller<NavBannerController> context,
            IImageService imageService,
            ILanguageService languageService,
            INavBannerService navBannerService,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(imageService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _imageService = imageService;
            _languageService = languageService;
            _navBannerService = navBannerService;
            _permissionGroupService = permissionGroupService;
        }

        [HttpGet]
        [Route("{action}")]
        public async Task<IActionResult> BannerImage(int navBannerId, string languageName)
        {
            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(languageName, StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

            var navBannerImage = await _navBannerService
                .GetImageByNavBannerIdAsync(navBannerId, selectedLanguage.Id);

            if (navBannerImage == null)
            {
                return StatusCode(404);
            }

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

                if (navBanner == null)
                {
                    ShowAlertDanger($"No nav banner found with id {id}.");
                    return RedirectToAction(nameof(PagesController.Index), PagesController.Name);
                }

                var languages = await _languageService.GetActiveAsync();

                var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

                var navBannerImage = await _navBannerService
                    .GetImageByNavBannerIdAsync(id, selectedLanguage.Id)
                    ?? new NavBannerImage();

                var navBannerLinks = (await _navBannerService
                    .GetLinksByNavBannerIdAsync(navBanner.Id, selectedLanguage.Id));

                if (navBannerLinks.Count == 0)
                {
                    navBannerLinks = new List<NavBannerLink>
                        {
                            new() { Text = new NavBannerLinkText() },
                            new() { Text = new NavBannerLinkText() },
                            new() { Text = new NavBannerLinkText() },
                            new() { Text = new NavBannerLinkText() }
                        };
                }
                else if (navBannerLinks.Any(_ => _.Text == null))
                {
                    foreach (var link in navBannerLinks)
                    {
                        link.Text = new NavBannerLinkText();
                    }
                }

                var viewModel = new DetailViewModel
                {
                    Name = navBanner.Name,
                    LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                    PageLayoutId = await _navBannerService.GetPageLayoutIdForNavBannerAsync(navBanner.Id),
                    NavBannerId = id,
                    NavBannerImage = navBannerImage,
                    Links = navBannerLinks,
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
            else
            {
                viewModel.Language = await _languageService.GetActiveByIdAsync(viewModel.Language.Id);
            }

            var navBanner = await _navBannerService.GetByIdAsync(viewModel.NavBannerId);

            if (navBanner == null)
            {
                ShowAlertDanger($"No nav banner found with id {viewModel.NavBannerId}.");

                return RedirectToAction(
                    nameof(PagesController.LayoutDetail), 
                    PagesController.Name, 
                    new { id = viewModel.PageLayoutId });
            }

            var navBannerImage = await _navBannerService
                .GetImageByNavBannerIdAsync(viewModel.NavBannerId, viewModel.Language.Id);

            var deleteImageFilePath = "";

            if (viewModel.Image != null)
            {
                if (navBannerImage != null)
                {

                    var oldFileName = Path.GetFileName(navBannerImage.ImagePath);

                    if (viewModel.Image.FileName != oldFileName)
                    {
                        deleteImageFilePath = await _navBannerService.GetUploadImageFilePathAsync(
                            viewModel.Language.Name,
                            oldFileName);
                    }

                    navBannerImage.ImagePath = _navBannerService.GetImageAssetPath(
                        viewModel.Image.FileName,
                        viewModel.Language.Name);

                    navBannerImage.ImageAltText = string.IsNullOrEmpty(viewModel.NavBannerImage?.ImageAltText)
                        ? navBannerImage.ImageAltText
                        : viewModel.NavBannerImage.ImageAltText;

                    try
                    {
                        await OptimizeAndSaveImageFileAsync(viewModel.Image, viewModel.Language);

                        _navBannerService.UpdateImageNoSave(navBannerImage);
                    }
                    catch (ParameterException pex)
                    {
                        ModelState.AddModelError(
                            nameof(viewModel.Image), 
                            $"Error optimizing/saving uploaded image. {pex.Message}");

                        return View(viewModel);
                    }

                }
                else
                {
                    try
                    {
                        await OptimizeAndSaveImageFileAsync(viewModel.Image, viewModel.Language);

                        viewModel.NavBannerImage.ImagePath = _navBannerService.GetImageAssetPath(
                            viewModel.Image.FileName,
                            viewModel.Language.Name);

                        viewModel.NavBannerImage.NavBannerId = navBanner.Id;
                        viewModel.NavBannerImage.ImageAltText = viewModel.NavBannerImage.ImageAltText;
                        viewModel.NavBannerImage.LanguageId = viewModel.Language.Id;

                        await _navBannerService.AddImageNoSaveAsync(viewModel.NavBannerImage);
                    }
                    catch (ParameterException pex)
                    {
                        ShowAlertDanger($"Error optimizing uploaded image. {pex.Message}");
                        return View(viewModel);
                    }

                }
            }
            else if (navBannerImage != null && !string.IsNullOrEmpty(viewModel.NavBannerImage.ImageAltText))
            {
                navBannerImage.ImageAltText = viewModel.NavBannerImage.ImageAltText;
                _navBannerService.UpdateImageNoSave(navBannerImage);
            }
            else if (navBannerImage == null)
            {
                ModelState.AddModelError(nameof(viewModel.Image), "Please provide an image for the Nav Banner");
                ShowAlertDanger("Please provide an image for the Nav Banner");
                return View(viewModel);
            }

            if (viewModel.Links != null)
            {
                var navBannerLinks = await _navBannerService.GetLinksByNavBannerIdAsync(
                    navBanner.Id,
                    viewModel.Language.Id)
                    ?? new List<NavBannerLink>();

                if (navBannerLinks.Count == 0)
                {
                    for (int i = 0; i < viewModel.Links.Count; i++)
                    {
                        navBannerLinks.Add(new NavBannerLink
                        {
                            Icon = viewModel.Links[i].Icon,
                            Link = viewModel.Links[i].Link,
                            NavBannerId = navBanner.Id,
                            Order = i,
                        });

                        navBannerLinks[i].Text = new NavBannerLinkText
                        {
                            Text = viewModel.Links[i].Text.Text,
                            LanguageId = viewModel.Language.Id,
                            NavBannerLink = navBannerLinks[i],
                        };
                    }

                    await _navBannerService.AddLinksAndTextsNoSaveAsync(navBannerLinks);
                }
                else
                {

                    if (navBannerLinks.Any(_ => _.Text == null))
                    {
                        var texts = viewModel.Links.Select(_ => _.Text).ToList();

                        for (int i = 0; i < texts.Count; i++)
                        {
                            texts[i].LanguageId = viewModel.Language.Id;
                            texts[i].NavBannerLinkId = navBannerLinks[i].Id;

                            navBannerLinks[i].Icon = string.IsNullOrEmpty(viewModel.Links[i].Icon)
                                ? navBannerLinks[i].Icon
                                : viewModel.Links[i].Icon;
                        }

                        await _navBannerService.AddLinkTextsNoSaveAsync(texts);
                    }
                    else
                    {
                        for (int i = 0; (i < viewModel.Links.Count); i++)
                        {
                            navBannerLinks[i].Icon = string.IsNullOrEmpty(viewModel.Links[i].Icon)
                                ? navBannerLinks[i].Icon
                                : viewModel.Links[i].Icon;
                            navBannerLinks[i].Link = string.IsNullOrEmpty(viewModel.Links[i].Link)
                                ? navBannerLinks[i].Link
                                : viewModel.Links[i].Link;
                            navBannerLinks[i].Text.Text = string.IsNullOrEmpty(viewModel.Links[i].Text?.Text)
                                ? navBannerLinks[i].Text.Text
                                : viewModel.Links[i].Text?.Text;

                            _navBannerService.UpdateLinkTextNoSave(navBannerLinks[i].Text);
                        }
                    }

                    _navBannerService.UpdateLinksNoSave(navBannerLinks);
                }

            }

            await _navBannerService.SaveAsync();

            if (!string.IsNullOrEmpty(deleteImageFilePath) && Path.Exists(deleteImageFilePath))
            {
                System.IO.File.Delete(deleteImageFilePath);
            }

            return RedirectToAction(
                nameof(PagesController.LayoutDetail), 
                PagesController.Name, 
                new { id = viewModel.PageLayoutId });
        }

        private async Task OptimizeAndSaveImageFileAsync(IFormFile image, Language language)
        {
            OptimizedImageResult optimized;
            byte[] imageBytes;

            try
            {
                optimized = await _imageService.OptimizeAsync(image);
                imageBytes = optimized.File;

                // get an approved filename with path
                var filename = await _navBannerService.GetUploadImageFilePathAsync(language.Name,
                    image.FileName);

                // copy file
                await System.IO.File.WriteAllBytesAsync(filename, imageBytes);

            }
            catch (ParameterException pex)
            {
                ModelState.AddModelError("ItemImage",
                    $"Error optimizing uploaded image: {pex.Message}");
                throw pex;
            }
        }

        // TODO: Figure out how this works and make sure it's implemented correctly
        private async Task<bool> HasNavBannerPermissionAsync(int navBannerId)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    var pageHeaderId = await _navBannerService.GetPageHeaderIdAsync(navBannerId);
                    if (pageHeaderId.HasValue)
                    {
                        var permissionGroups = await _permissionGroupService
                            .GetPermissionsAsync<PermissionGroupPageContent>(pageHeaderId.Value);
                        var permissionGroupsStrings = permissionGroups
                            .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                        if (permissionClaims.Any(_ => permissionGroupsStrings.Contains(_)))
                        {
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("No permission for {Username} ({UserId}) to edit nav banners, permissions: {PermissionList}",
                                CurrentUsername,
                                CurrentUserId,
                                string.Join(", ", permissionGroupsStrings));
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No page header found for nav banner id {NavBannerId}", navBannerId);
                    }
                }
                else
                {
                    _logger.LogWarning("No claims for {UserName} ({UserId}) to edit nav banners.",
                        CurrentUsername,
                        CurrentUserId);
                }
                return false;
            }
        }
    }
}
