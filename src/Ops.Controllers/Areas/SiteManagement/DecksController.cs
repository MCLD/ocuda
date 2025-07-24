using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Decks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class DecksController : BaseController<DecksController>
    {
        public static readonly int CardImageWidth = 666;
        public static readonly int MaximumFileSizeBytes = 0; // disable this check
        private static readonly string[] ValidImageExtensions = new[] { ".jpg", ".png" };

        private readonly IDeckService _deckService;
        private readonly IImageService _imageService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;

        public DecksController(ServiceFacades.Controller<DecksController> context,
            IDeckService deckService,
            IImageService imageService,
            ILanguageService languageService,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(deckService);
            ArgumentNullException.ThrowIfNull(imageService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _deckService = deckService;
            _imageService = imageService;
            _languageService = languageService;
            _permissionGroupService = permissionGroupService;
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Decks"; } }

        [HttpGet("[action]/{deckId}")]
        public async Task<IActionResult> AddCard(int deckId, CardViewModel viewModel)
        {
            if (!await HasDeckPermissionAsync(deckId))
            {
                return RedirectToUnauthorized();
            }

            var deck = await _deckService.GetByIdAsync(deckId);

            var languages = await _languageService.GetActiveAsync();

            viewModel ??= new CardViewModel();

            viewModel.SelectedLanguage = languages.Single(_ => _.IsDefault);
            viewModel.BackLink = Url.Action(nameof(Detail), new { deckId });
            viewModel.DeckId = deckId;
            viewModel.DeckName = deck.Name;
            viewModel.LanguageDescription = viewModel.SelectedLanguage.Description;
            viewModel.LanguageId = viewModel.SelectedLanguage.Id;

            return View("Card", viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddUpdateImage(CardViewModel viewModel)
        {
            var issues = new List<string>();
            string languageName = null;

            if (viewModel == null)
            {
                return Json(new JsonResponse
                {
                    Message = "Invalid request",
                    Success = false
                });
            }

            if (!await HasDeckPermissionAsync(viewModel.DeckId))
            {
                issues.Add("Permission denied");
            }
            else if (!ModelState.IsValid)
            {
                issues.AddRange(ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage));
            }
            else if (viewModel.CardImage == null)
            {
                issues.Add("You must supply an image to upload.");
            }
            else if (!ValidImageExtensions.Contains(Path.GetExtension(viewModel.CardImage.FileName)))
            {
                issues.Add($"Image type must be one of: {string.Join(", ", ValidImageExtensions)}");
            }
            else if (MaximumFileSizeBytes > 0 && viewModel.CardImage.Length > MaximumFileSizeBytes)
            {
                issues.Add($"Image must be smaller than {MaximumFileSizeBytes / 1024} KB");
            }
            else
            {
                byte[] imageBytes = null;
                OptimizedImageResult optimized;

                try
                {
                    optimized = await _imageService.OptimizeAsync(viewModel.CardImage);
                    imageBytes = optimized.File;
                }
                catch (ParameterException pex)
                {
                    issues.Add($"Error optimizing uploaded image: {pex.Message}");
                }
                catch (OcudaConfigurationException)
                { }

                if (imageBytes == null)
                {
                    await using var memoryStream = new MemoryStream();
                    await viewModel.CardImage.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                if (imageBytes != null)
                {
                    using var image = SixLabors.ImageSharp.Image.Load(imageBytes);
                    if (image.Width != CardImageWidth)
                    {
                        issues.Add($"Card images must be {CardImageWidth} pixels wide.");
                    }
                    else
                    {
                        var language = await _languageService
                            .GetActiveByIdAsync(viewModel.CardDetail.LanguageId);
                        languageName = language.Name;

                        var currentCard = await _deckService
                            .GetCardDetailsAsync(viewModel.CardDetail.CardId,
                                viewModel.CardDetail.LanguageId);

                        var currentFilename = Path.GetFileName(currentCard.Filename);

                        // file is the same then replace the old one
                        bool replaceImage = currentFilename?.Equals(viewModel.CardImage.FileName,
                            StringComparison.OrdinalIgnoreCase) == true;

                        // filename is different but there is an old image, delete it
                        if (!replaceImage && !string.IsNullOrEmpty(currentFilename))
                        {
                            await _deckService.RemoveCardImageAsync(currentCard.CardId,
                                currentCard.LanguageId);
                        }

                        // get an approved filename with path
                        var filename = await _deckService.GetUploadImageFilePathAsync(languageName,
                            viewModel.CardImage.FileName,
                            replaceImage);

                        // copy file
                        await System.IO.File.WriteAllBytesAsync(filename, imageBytes);

                        // update detail
                        currentCard.Filename = Path.GetFileName(filename);
                        currentCard.AltText = viewModel.AltText;
                        await _deckService.UpdateCardAsync(currentCard.CardId,
                            currentCard.LanguageId,
                            currentCard);
                    }
                }
            }

            if (issues.Count > 0)
            {
                return Json(new JsonResponse
                {
                    Message = string.Concat(issues.Select(_ => "<li>" + _ + "</li>")),
                    Success = false
                });
            }
            else
            {
                return Json(new JsonResponse
                {
                    Success = true,
                    Url = Url.Action(nameof(UpdateCard), new
                    {
                        cardId = viewModel.CardDetail.CardId,
                        language = languageName
                    })
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CardOrderDecrement(int cardId)
        {
            int? deckId = null;
            try
            {
                deckId = await _deckService.CardOrderAsync(cardId, false);
            }
            catch (OcudaException oex)
            {
                ShowAlertWarning(oex.Message);
            }
            return deckId.HasValue
                ? RedirectToAction(nameof(Detail), new { deckId })
                : RedirectToAction(nameof(PagesController.Index), PagesController.Name);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CardOrderIncrement(int cardId)
        {
            int? deckId = null;
            try
            {
                deckId = await _deckService.CardOrderAsync(cardId, true);
            }
            catch (OcudaException oex)
            {
                ShowAlertWarning(oex.Message);
            }
            return deckId.HasValue
                ? RedirectToAction(nameof(Detail), new { deckId })
                : RedirectToAction(nameof(PagesController.Index), PagesController.Name);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteCard(int cardId)
        {
            (int deckId, int pageLayoutId) = await _deckService.DeleteCardAsync(cardId);

            if (deckId == 0 && pageLayoutId == 0)
            {
                _logger.LogInformation("Card {Card} (last in deck and layout) removed", cardId);
                return RedirectToAction(nameof(PagesController.Index), PagesController.Name);
            }
            else if (deckId == 0)
            {
                _logger.LogInformation("Card {Card} (last in deck) removed", cardId);
                return RedirectToAction(
                    nameof(PagesController.LayoutDetail),
                    PagesController.Name,
                    new
                    {
                        id = pageLayoutId
                    });
            }
            else
            {
                _logger.LogInformation("Card {Card} from deck {Deck} in layout {Layout} removed",
                    cardId,
                    deckId,
                    pageLayoutId);
                return RedirectToAction(nameof(Detail), new { deckId });
            }
        }

        [HttpGet("[action]/{deckId}")]
        public async Task<IActionResult> Detail(int deckId, string language)
        {
            if (!await HasDeckPermissionAsync(deckId))
            {
                return RedirectToUnauthorized();
            }

            var deck = await _deckService.GetByIdAsync(deckId);

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var viewModel = new DetailViewModel
            {
                BackLink = Url.Action(nameof(PagesController.LayoutDetail),
                PagesController.Name,
                new
                {
                    id = await _deckService.GetPageLayoutIdAsync(deckId)
                }),
                DeckId = deckId,
                DeckName = deck.Name,
                SelectedLanguage = selectedLanguage,
                LanguageList = new SelectList(languages,
                nameof(Language.Name),
                nameof(Language.Description),
                selectedLanguage.Name)
            };

            viewModel.CardDetails.AddRange(await _deckService
                .GetCardDetailsByDeckAsync(deckId, selectedLanguage.Id));

            if (viewModel.CardDetails != null)
            {
                foreach (var card in viewModel.CardDetails)
                {
                    if (!string.IsNullOrEmpty(card.Footer))
                    {
                        card.Footer = CommonMark.CommonMarkConverter.Convert(card.Footer);
                    }
                }
            }

            return View(viewModel);
        }

        [HttpGet("[action]/{language}/{filename}")]
        public async Task<IActionResult> Image(string language, string filename)
        {
            var basePath = await _deckService.GetFullImageDirectoryPath(language);
            var filePath = Path.Combine(basePath, filename);
            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(404);
            }
            else
            {
                new FileExtensionContentTypeProvider()
                    .TryGetContentType(filePath, out string fileType);

                return PhysicalFile(filePath, fileType
                    ?? System.Net.Mime.MediaTypeNames.Application.Octet);
            }
        }

        [HttpPost("[action]/{language}/{cardId}")]
        public async Task<IActionResult> UpdateCard(string language,
            int cardId,
            CardViewModel viewModel)
        {
            // update
            if (viewModel == null
                || !await HasDeckPermissionAsync(viewModel.DeckId))
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(viewModel.CardDetail.AltText?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Header?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Link?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Text?.Trim()))
            {
                ShowAlertWarning("Cannot update empty card.");
                return RedirectToAction(nameof(Detail), new
                {
                    deckId = viewModel.DeckId
                });
            }

            if (!ModelState.IsValid)
            {
                var sb = new StringBuilder();
                foreach (var item in ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        sb.AppendLine(error.ErrorMessage);
                    }
                }
                ShowAlertWarning(sb.ToString());
                return RedirectToAction(nameof(AddCard), new
                {
                    deckId = viewModel.DeckId,
                    viewModel
                });
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            await _deckService.UpdateCardAsync(cardId,
                selectedLanguage.Id,
                viewModel.CardDetail);

            return RedirectToAction(nameof(Detail), new { deckId = viewModel.DeckId });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateCard(CardViewModel viewModel)
        {
            if (viewModel == null
                || !await HasDeckPermissionAsync(viewModel.DeckId))
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(viewModel.CardDetail.AltText?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Header?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Link?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Text?.Trim())
                && string.IsNullOrEmpty(viewModel.CardDetail.Footer?.Trim()))
            {
                ShowAlertWarning("Cannot create empty card.");
                return RedirectToAction(nameof(Detail), new
                {
                    deckId = viewModel.DeckId
                });
            }

            if (!ModelState.IsValid)
            {
                var sb = new StringBuilder();
                foreach (var item in ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        sb.AppendLine(error.ErrorMessage);
                    }
                }
                ShowAlertWarning(sb.ToString());
                return RedirectToAction(nameof(AddCard), new
                {
                    deckId = viewModel.DeckId,
                    viewModel
                });
            }

            await _deckService.AddCardAndDetailAsync(viewModel.DeckId,
                viewModel.LanguageId,
                viewModel.CardDetail);

            return RedirectToAction(nameof(Detail), new { deckId = viewModel.DeckId });
        }

        [HttpGet("[action]/{language}/{cardId}")]
        public async Task<IActionResult> UpdateCard(string language, int cardId)
        {
            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var card = await _deckService.GetCardDetailsAsync(cardId, selectedLanguage.Id);

            int deckId = card?.Card?.DeckId
                ?? await _deckService.GetDeckIdAsync(cardId);

            bool isUpdate = card != null;

            card ??= new CardDetail
            {
                CardId = cardId,
                LanguageId = selectedLanguage.Id
            };

            var cardCount = await _deckService.GetCardCountAsync(deckId);

            return View("Card", new CardViewModel
            {
                BackLink = Url.Action(nameof(Detail), new { deckId }),
                CardDetail = card,
                DeckId = deckId,
                IsOnlyCard = cardCount == 1,
                IsUpdate = isUpdate,
                LanguageDescription = selectedLanguage.Description,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                SelectedLanguage = selectedLanguage
            });
        }

        private async Task<bool> HasDeckPermissionAsync(int deckId)
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
                    var pageHeaderId = await _deckService.GetPageHeaderIdAsync(deckId);
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
                            _logger.LogWarning("No permission for {Username} ({UserId}) to edit decks, permissions: {PermissionList}",
                                CurrentUsername,
                                CurrentUserId,
                                string.Join(", ", permissionGroupsStrings));
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No page header found for deck id {DeckId}", deckId);
                    }
                }
                else
                {
                    _logger.LogWarning("No claims for {UserName} ({UserId}) to edit decks.",
                        CurrentUsername,
                        CurrentUserId);
                }
                return false;
            }
        }
    }
}