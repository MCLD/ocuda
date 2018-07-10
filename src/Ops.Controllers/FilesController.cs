using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Files;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class FilesController : BaseController
    {
        private readonly ILogger<FilesController> _logger;
        private readonly CategoryService _categoryService;
        private readonly FileService _fileService;
        private readonly SectionService _sectionService;

        public FilesController(ILogger<FilesController> logger,
            ServiceFacade.Controller context,
            CategoryService categoryService,
            FileService fileService,
            SectionService sectionService
            ) : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int? categoryId = null, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page)
            {
                SectionId = currentSection.Id,
                CategoryId = categoryId,
                CategoryType = CategoryType.File
            };

            var fileList = await _fileService.GetPaginatedListAsync(filter);
            var categoryList = await _categoryService.GetBySectionIdAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = fileList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
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
                Files = fileList.Data,
                Categories = categoryList
            };

            if (categoryId.HasValue)
            {
                viewModel.CategoryName =
                    (await _categoryService.GetCategoryByIdAsync(categoryId.Value)).Name;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> ViewFile(int id)
        {
            var file = await _fileService.GetByIdAsync(id);
            var extension = System.IO.Path.GetExtension(file.FilePath);
            var fileName = $"{file.Name}{extension}";
            byte[] fileBytes;

            try
            {
                using (var fileStream = System.IO.File.OpenRead(file.FilePath))
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                }

                var typeProvider = new FileExtensionContentTypeProvider();
                typeProvider.TryGetContentType(file.FilePath, out string fileType);

                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(fileBytes, fileType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error viewing file {file.Id} : {ex.Message}", ex);
                return StatusCode(404);
            }
        }
    }
}
