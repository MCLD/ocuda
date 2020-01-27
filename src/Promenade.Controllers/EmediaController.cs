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
            var emediaViewModel = new EmediaViewModel
            {
                AllEmedia = await _emediaService.GetAllEmediaAsync()
            };

            foreach (var emedia in emediaViewModel.AllEmedia)
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

            PageTitle = "eMedia";

            return View(emediaViewModel);
        }
    }
}
