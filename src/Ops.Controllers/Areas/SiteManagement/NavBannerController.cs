using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
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
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class NavBannerController : BaseController<NavBannerController>
    {
        private const int NumberOfNavs = 4;
        private readonly IImageService _imageService;
        private readonly ILanguageService _languageService;
        private readonly INavBannerService _navBannerService;
        private readonly IPermissionGroupService _permissionGroupService;

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

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "NavBanner"; } }

        [HttpGet]
        [Route("{action}")]
        [ResponseCache(Duration = 0, NoStore = true)]
        public async Task<IActionResult> BannerImage(int navBannerId, string languageName)
        {
            if (!await HasNavBannerPermissionAsync(navBannerId))
            {
                return RedirectToUnauthorized();
            }

            var promBasePath = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(languageName,
                        StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

            var navBannerImage = await _navBannerService
                .GetImageByNavBannerIdAsync(navBannerId, selectedLanguage.Id);

            if (navBannerImage == null)
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(navBannerImage.ImageFilePath))
            {
                return NotFound();
            }
            else
            {
                new FileExtensionContentTypeProvider()
                    .TryGetContentType(navBannerImage.ImageFilePath, out string fileType);

                return PhysicalFile(navBannerImage.ImageFilePath, fileType
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
                    ShowAlertDanger($"No NavBanner found with id {id}.");
                    return RedirectToAction(nameof(PagesController.Index), PagesController.Name);
                }

                var languages = await _languageService.GetActiveAsync();

                var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(language,
                        StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

                var navBannerImage = await _navBannerService
                    .GetImageByNavBannerIdAsync(id, selectedLanguage.Id)
                    ?? new NavBannerImage();

                var viewModel = new DetailViewModel
                {
                    Language = selectedLanguage,
                    LanguageList = new SelectList(languages,
                        nameof(Language.Name),
                        nameof(Language.Description),
                        selectedLanguage.Name),
                    Name = navBanner.Name,
                    NavBannerId = id,
                    NavBannerImage = navBannerImage,
                    NavBannerLink = Url.Action(nameof(BannerImage)),
                    PageLayoutId = await _navBannerService
                        .GetPageLayoutIdForNavBannerAsync(navBanner.Id)
                };

                var navBannerLinks = await _navBannerService
                    .GetLinksByNavBannerIdAsync(navBanner.Id, selectedLanguage.Id);

                for (int order = 0; order < NumberOfNavs; order++)
                {
                    var navBannerLink = navBannerLinks.SingleOrDefault(_ => _.Order == order);
                    if (navBannerLink == null)
                    {
                        navBannerLink = new NavBannerLink
                        {
                            Order = order,
                            NavBannerLinkText = new NavBannerLinkText()
                        };
                    }
                    else
                    {
                        navBannerLink.NavBannerLinkText ??= new NavBannerLinkText();
                    }

                    switch (order)
                    {
                        case 0:
                            viewModel.TopLeft = navBannerLink;
                            break;

                        case 1:
                            viewModel.TopRight = navBannerLink;
                            break;

                        case 2:
                            viewModel.BottomLeft = navBannerLink;
                            break;

                        case 3:
                            viewModel.BottomRight = navBannerLink;
                            break;
                    }
                }

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
                return RedirectToAction(nameof(Index));
            }

            if (!await HasNavBannerPermissionAsync(viewModel.NavBannerId))
            {
                return RedirectToUnauthorized();
            }

            if (viewModel.Language == null)
            {
                var languages = await _languageService.GetActiveAsync();

                viewModel.Language = languages.Single(_ => _.IsDefault);
            }
            else
            {
                viewModel.Language
                    = await _languageService.GetActiveByIdAsync(viewModel.Language.Id);
            }

            var navBanner = await _navBannerService.GetByIdAsync(viewModel.NavBannerId);

            if (navBanner == null)
            {
                ShowAlertDanger($"No NavBanner found with id {viewModel.NavBannerId}.");

                return RedirectToAction(
                    nameof(PagesController.LayoutDetail),
                    PagesController.Name,
                    new { id = viewModel.PageLayoutId });
            }

            var navBannerImage = await _navBannerService
                .GetImageByNavBannerIdAsync(viewModel.NavBannerId, viewModel.Language.Id);

            if (viewModel.Image != null)
            {
                // new image supplied
                var navBannerImagePath
                    = await _navBannerService.GetUploadImageFilePathAsync(viewModel.Language.Name);

                string deleteImageFilePath = null;
                string uploadImageFilename = viewModel.Image.FileName.Trim();
                string saveImageFilename = uploadImageFilename;

                var uploadFilenameUseCount = await _navBannerService
                    .ImageUseCountAsync(viewModel.Language.Id, uploadImageFilename);

                if (navBannerImage == null)
                {
                    // first image, new upload
                    if (uploadFilenameUseCount > 0)
                    {
                        // new image, in-use filename
                        saveImageFilename = FileHelper.GetUniqueFilename(navBannerImagePath,
                            uploadImageFilename);
                    }
                }
                else
                {
                    // if upload filename is in use, select a new one
                    if (uploadFilenameUseCount > 1)
                    {
                        // used more than once, generate new
                        saveImageFilename = FileHelper.GetUniqueFilename(navBannerImagePath,
                            uploadImageFilename);
                    }

                    if (navBannerImage.Filename != uploadImageFilename)
                    {
                        // new filename, remove old file if this was only use
                        var existingFilenameUseCount = await _navBannerService
                            .ImageUseCountAsync(viewModel.Language.Id, navBannerImage.Filename);
                        if (existingFilenameUseCount == 1)
                        {
                            deleteImageFilePath = Path.Combine(navBannerImagePath,
                                navBannerImage.Filename);
                        }
                    }
                }

                _logger.LogInformation("Image for {NavBannerId} {Status} uploaded {UploadFilename} in use {Times} times, saved as {SavedFilename}",
                    viewModel.NavBannerId,
                    navBannerImage == null ? "new" : "updated",
                    uploadImageFilename,
                    uploadFilenameUseCount,
                    saveImageFilename);
                var optimizeResult = await OptimizeAndSaveImageFileAsync(viewModel.Image,
                    Path.Combine(navBannerImagePath, saveImageFilename));

                if (!optimizeResult)
                {
                    AlertWarning = "Unable to optimize image, uploaded un-optimized";
                }

                if (navBannerImage != null)
                {
                    navBannerImage.Filename = saveImageFilename;
                    navBannerImage.ImageAltText
                        = string.IsNullOrEmpty(viewModel.NavBannerImage?.ImageAltText)
                            ? navBannerImage.ImageAltText
                            : viewModel.NavBannerImage.ImageAltText;

                    _navBannerService.UpdateImageNoSave(navBannerImage);
                }
                else
                {
                    viewModel.NavBannerImage.Filename = saveImageFilename;
                    viewModel.NavBannerImage.NavBannerId = navBanner.Id;
                    viewModel.NavBannerImage.ImageAltText = viewModel.NavBannerImage.ImageAltText;
                    viewModel.NavBannerImage.LanguageId = viewModel.Language.Id;

                    await _navBannerService.AddImageNoSaveAsync(viewModel.NavBannerImage);
                }

                if (!string.IsNullOrEmpty(deleteImageFilePath)
                    && System.IO.File.Exists(deleteImageFilePath))
                {
                    _logger.LogInformation("Deleting now-unused image: {Path}",
                        deleteImageFilePath);
                    System.IO.File.Delete(deleteImageFilePath);
                }
            }
            else if (navBannerImage != null
                && !string.IsNullOrEmpty(viewModel.NavBannerImage.ImageAltText))
            {
                navBannerImage.ImageAltText = viewModel.NavBannerImage.ImageAltText;
                _navBannerService.UpdateImageNoSave(navBannerImage);
            }
            else if (navBannerImage == null)
            {
                ModelState.AddModelError(nameof(viewModel.Image),
                    "Please provide an image for the NavBanner");
                ShowAlertDanger("Please provide an image for the NavBanner");
                return View(viewModel);
            }

            var submittedLinks = new[]
            {
                viewModel.TopLeft,
                viewModel.TopRight,
                viewModel.BottomLeft,
                viewModel.BottomRight
            };

            var navBannerLinks = await _navBannerService.GetLinksByNavBannerIdAsync(
                navBanner.Id,
                viewModel.Language.Id)
                ?? new List<NavBannerLink>();

            for (int order = 0; order < NumberOfNavs; order++)
            {
                var newLink = submittedLinks.SingleOrDefault(_ => _.Order == order);
                var existingLink = navBannerLinks.SingleOrDefault(_ => _.Order == order);

                if (existingLink == null)
                {
                    newLink.NavBannerId = navBanner.Id;
                    await _navBannerService.AddLinkAsync(newLink);
                    await _navBannerService.AddLinkTextAsync(new NavBannerLinkText
                    {
                        Text = newLink.NavBannerLinkText.Text,
                        LanguageId = viewModel.Language.Id,
                        NavBannerLinkId = newLink.Id
                    });
                }
                else
                {
                    existingLink.Icon = newLink.Icon?.Trim();
                    existingLink.Link = newLink.Link?.Trim();
                    if (existingLink.NavBannerLinkText == null)
                    {
                        existingLink.NavBannerLinkText = new NavBannerLinkText
                        {
                            LanguageId = viewModel.Language.Id,
                            NavBannerLinkId = existingLink.Id,
                            Text = newLink.NavBannerLinkText.Text?.Trim()
                        };
                        await _navBannerService.AddLinkTextAsync(existingLink.NavBannerLinkText);
                    }
                    else
                    {
                        existingLink.NavBannerLinkText.Text
                            = newLink.NavBannerLinkText.Text?.Trim();
                        await _navBannerService.UpdateLinkTextAsync(existingLink.NavBannerLinkText);
                    }
                    await _navBannerService.UpdateLinkAsync(existingLink);
                }
            }

            return RedirectToAction(
                nameof(PagesController.LayoutDetail),
                PagesController.Name,
                new { id = viewModel.PageLayoutId });
        }

        private async Task<bool> HasNavBannerPermissionAsync(int navBannerId)
        {
            if (IsSiteManager() || await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.WebPageContentManagement))
            {
                return true;
            }

            var permissionClaims = UserClaims(ClaimType.PermissionId);
            if (permissionClaims.Count == 0)
            {
                return false;
            }

            var pageHeaderId = await _navBannerService.GetPageHeaderIdAsync(navBannerId);
            if (!pageHeaderId.HasValue)
            {
                return false;
            }

            var permissionGroups = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupPageContent>(pageHeaderId.Value);
            var permissionGroupsStrings = permissionGroups
                .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

            return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
        }

        private async Task<bool> OptimizeAndSaveImageFileAsync(IFormFile image, string path)
        {
            bool success = true;
            byte[] imageBytes = null;
            OptimizedImageResult optimized;

            try
            {
                optimized = await _imageService.OptimizeAsync(image);
                imageBytes = optimized.File;
            }
            catch (ParameterException)
            {
                success = false;
            }
            catch (OcudaConfigurationException)
            { }

            if (imageBytes == null)
            {
                await using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            if (imageBytes == null)
            {
                throw new OcudaException("Unable to process image.");
            }

            // copy file
            await System.IO.File.WriteAllBytesAsync(path, imageBytes);

            return success;
        }
    }
}