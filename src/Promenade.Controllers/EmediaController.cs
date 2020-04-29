using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Emedias;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class EmediaController : BaseController<EmediaController>
    {
        private readonly EmediaService _emediaService;

        public EmediaController(ServiceFacades.Controller<EmediaController> context,
            EmediaService emediaService) : base(context)
        {
            _emediaService = emediaService ?? throw new ArgumentNullException(nameof(emediaService));
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
                    group.Segment.SegmentText.Text = CommonMark.CommonMarkConverter
                        .Convert(group.Segment.SegmentText.Text);
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

            var emediaViewModel = new EmediaViewModel
            {
                GroupedEmedia = groupedEmedia
            };

            PageTitle = "eMedia";

            return View(emediaViewModel);
        }
    }
}
