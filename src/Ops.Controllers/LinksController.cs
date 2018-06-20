using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Links;
using Ocuda.Ops.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class LinksController : BaseController
    {
        private readonly LinkService _linkService;

        public LinksController(LinkService linkService)
        {
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var linkList = await _linkService.GetLinksAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _linkService.GetLinkCountAsync(),
                CurrentPage = page,
                ItemsPerPage = 2
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Links = linkList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }
    }
}
