using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.DigitalLibrary;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("digital-library")]
    [Route("{culture:cultureConstraint?}/digital-library")]
    public class DigitalLibraryController : BaseController<DigitalLibraryController>
    {
        private readonly EmediaService _emediaService;
        private readonly SocialCardService _socialCardService;

        public DigitalLibraryController(ServiceFacades.Controller<DigitalLibraryController> context,
            EmediaService emediaService,
            SocialCardService socialCardService) : base(context)
        {
            _emediaService = emediaService
                ?? throw new ArgumentNullException(nameof(emediaService));
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        [Route("")]
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

            PageTitle = "eMedia";

            return View(emediaViewModel);
        }
    }
}