using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Files;
using Ocuda.Ops.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class FilesController : BaseController
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var fileList = await _fileService.GetFilesAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _fileService.GetFileCountAsync(),
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
                Files = fileList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }
    }
}
