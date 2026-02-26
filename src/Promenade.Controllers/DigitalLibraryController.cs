using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.DigitalLibrary;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("digital-library")]
    [Route("{culture:cultureConstraint?}/digital-library")]
    public class DigitalLibraryController(ServiceFacades.Controller<DigitalLibraryController> context,
        EmediaService emediaService,
        SocialCardService socialCardService) : BaseController<DigitalLibraryController>(context)
    {
        private readonly EmediaService _emediaService = emediaService
            ?? throw new ArgumentNullException(nameof(emediaService));

        private readonly SocialCardService _socialCardService = socialCardService
            ?? throw new ArgumentNullException(nameof(socialCardService));

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var groupedEmedia = await _emediaService.GetGroupedEmediaAsync(forceReload);

            foreach (var group in groupedEmedia)
            {
                if (!string.IsNullOrWhiteSpace(group.Segment?.SegmentText?.Text))
                {
                    group.Segment.SegmentText.Text = FormatForDisplay(group.Segment.SegmentText);
                }

                foreach (var emedia in group.Emedias)
                {
                    if (!string.IsNullOrWhiteSpace(emedia.EmediaText?.Description))
                    {
                        emedia.EmediaText.Description = CommonMark.CommonMarkConverter
                            .Convert(emedia.EmediaText.Description);
                    }
                    if (!string.IsNullOrWhiteSpace(emedia.EmediaText?.Details))
                    {
                        emedia.EmediaText.Details = CommonMark.CommonMarkConverter
                            .Convert(emedia.EmediaText?.Details);
                    }
                }
            }

            var emediaSocial = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Social.EmediaCardId, forceReload);

            Models.Entities.SocialCard card = null;

            if (emediaSocial > -1)
            {
                card = await _socialCardService.GetByIdAsync(emediaSocial, forceReload);
            }

            var emediaViewModel = new DigitalLibraryViewModel
            {
                GroupedEmedia = groupedEmedia,
                SocialCard = card
            };

            PageTitle = "Digital Library";

            return View(emediaViewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Launch(string id)
        {
            var emedia = await _emediaService.GetAsync(id);

            if (emedia == null)
            {
                return NotFound();
            }

            if (emedia.IsHttpPost)
            {
                var launchViewModel = new LaunchViewModel
                {
                    Name = emedia.Name,
                    Uri = new Uri(emedia.RedirectUrl)
                };
                foreach (var (s, sv) in QueryHelpers.ParseQuery(launchViewModel.Uri.Query))
                {
                    launchViewModel.QueryStringValues.Add(s, sv);
                }
                return View(launchViewModel);
            }
            else
            {
                return Redirect(emedia.RedirectUrl);
            }
        }
    }
}